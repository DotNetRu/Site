#addin nuget:?package=HtmlAgilityPack&version=1.11.9

#load "./SiteMetaInformation.cake"

using System.IO;
using System.Text;
using System.Globalization;

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

        var (date, newsFile) = FindFreeSlot();

        var content = FormatNews(date, metaInfo);

        logger.Information($"Create news file: {newsFile.GetFilename().FullPath}");

        var uft8 = new UTF8Encoding(false);
        System.IO.File.WriteAllText(newsFile.FullPath, content, uft8);
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
$@"---
Title: {metaInfo.Title}
Author: {metaInfo.Author}
Link: {metaInfo.Url}
Image: {metaInfo.ImageUrl}
Tags: [{String.Join(", ", metaInfo.Tags)}]
Publisher: {publisherName}
PublishDate: {date:yyyy-MM-ddTHH:mm:ss}Z
---
{metaInfo.Description}
";
    }
}
