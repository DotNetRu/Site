#addin nuget:?package=HtmlAgilityPack

using System.IO;
using System.Text;
using System.Globalization;
using HtmlAgilityPack;

public sealed class SiteMetaInformation
{
    public Uri Url { get; }
    public string Title { get; }
    public string Description { get; }
    public Uri ImageUrl { get; }
    public string Name { get; }

    public SiteMetaInformation(Uri url, string title, string description, Uri imageUrl, string name)
    {
        Url = url;
        Title = title;
        Description = description;
        ImageUrl = imageUrl;
        Name = name;
    }

    public static SiteMetaInformation FromUrl(Uri url)
    {
        var web = new HtmlWeb();
        var document = web.Load(url);
        var metaTags = document.DocumentNode.SelectNodes("//meta");

        if (metaTags == null)
        {
            return null;
        }

        Uri siteUrl = null;
        string siteTitle = null;
        string siteDescription = null;
        Uri siteImage = null;
        string siteName = null;

        foreach (var tag in metaTags)
        {
            var tagName = tag.Attributes["name"];
            var tagContent = tag.Attributes["content"];
            var tagProperty = tag.Attributes["property"];

            if (tagProperty != null && tagContent != null)
            {
                switch (tagProperty.Value.ToLower())
                {
                    case "og:title":
                        siteTitle = String.IsNullOrEmpty(siteTitle) ? tagContent.Value : siteTitle;
                        break;
                    case "og:url":
                        siteUrl = siteUrl == null ? new Uri(tagContent.Value) : siteUrl;
                        break;
                    case "og:image":
                        siteImage = siteImage == null ? new Uri(tagContent.Value) : siteImage;
                        break;
                    case "og:description":
                        siteDescription = String.IsNullOrEmpty(siteDescription) ? tagContent.Value : siteDescription;
                        break;
                    case "og:site_name":
                        siteName = String.IsNullOrEmpty(siteName) ? tagContent.Value : siteName;
                        break;
                }
            }
            // else if (tagName != null && tagContent != null)
            // {
            //     switch (tagName.Value.ToLower())
            //     {
            //         case "twitter:title":
            //             siteTitle = String.IsNullOrEmpty(siteTitle) ? tagContent.Value : siteTitle;
            //             break;
            //         case "twitter:image":
            //             siteImage = siteImage == null ? new Uri(tagContent.Value) : siteImage;
            //             break;
            //         case "twitter:description":
            //             siteDescription = String.IsNullOrEmpty(siteDescription) ? tagContent.Value : siteDescription;
            //             break;
            //     }
            // }
        }

        if (String.IsNullOrWhiteSpace(siteTitle))
        {
            return null;
        }

        return new SiteMetaInformation(url, siteTitle, siteDescription, siteImage, siteName);
    }
}

public sealed class NewsWriter
{
    ICakeLog logger;
    IFileSystem fileSystem;
    DirectoryPath newsDir;
    string publisherName;

    public NewsWriter(ICakeContext context, DirectoryPath newsDir, string publisherName)
    {
        logger = context.Log;
        fileSystem = context.FileSystem;
        this.newsDir = newsDir;
        this.publisherName = publisherName;
    }

    public void BootstrapUrl(Uri url)
    {
        var metaInfo = SiteMetaInformation.FromUrl(url);
        if (metaInfo == null)
        {
            logger.Warning($"Meta information not found for {url}");
        }

        var (date, newsFile) = FindFreeSlot();

        var content = FormatNews(date, metaInfo);

        logger.Information($"Create news file: {newsFile.GetFilename().FullPath}");

        System.IO.File.WriteAllText(newsFile.FullPath, content, Encoding.UTF8);
    }

    private (DateTime, FilePath) FindFreeSlot()
    {
        var date = DateTime.UtcNow;
        FilePath newsFile;

        var tries = 0;
        do
        {
            if (tries > 0)
            {
                date = date.AddMinutes(1);
            }

            var fileName = date.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture) + ".md";
            newsFile = newsDir.CombineWithFilePath(new FilePath(fileName));

        } while (fileSystem.Exist(newsFile));

        return (date, newsFile);
    }

    private string FormatNews(DateTime date, SiteMetaInformation metaInfo)
    {
        return
$@"Title: {metaInfo?.Title}
Author:
Link: {metaInfo?.Url}
Image: {metaInfo?.ImageUrl}
Tags: []
Publisher: {publisherName}
PublishDate: {date:yyyy-MM-ddTHH:mm:ss}Z
---
{metaInfo?.Description}
";
    }
}
