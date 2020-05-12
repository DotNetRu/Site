using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using Nuke.Common;

public sealed class SiteMetaInformation
{
    string title;
    string description;
    Uri imageUrl;
    string author;
    string siteName;
    string type;
    HashSet<string> tags;

    public SiteMetaInformation(Uri url)
    {
        Url = url;
        // TODO: Case Insensitive tags
        tags = new HashSet<string>();
    }

    public void Test()
    {
        DumpUrl("https://www.hanselman.com/blog/UpgradingAnExistingNETProjectFilesToTheLeanNewCSPROJFormatFromNETCore.aspx");
        
        static void DumpUrl(string url)
        {
            var metaInfo = FromUrl(new Uri(url));
            if (metaInfo == null)
            {
                Logger.Normal("Not found for: {0}", url);
            }
            else
            {
                DumpMeta(metaInfo);
            }

            Logger.Normal("---");
        }

        static void DumpMeta(SiteMetaInformation metaInfo)
        {
            Logger.Normal("Uri: {0}", metaInfo.Url);
            Logger.Normal("Title: {0}", metaInfo.Title);
            Logger.Normal("Description: {0}", metaInfo.Description);
            Logger.Normal("ImageUrl: {0}", metaInfo.ImageUrl);
            Logger.Normal("Author: {0}", metaInfo.Author);
            Logger.Normal("Name: {0}", metaInfo.SiteName);
            Logger.Normal("Type: {0}", metaInfo.Type);
            Logger.Normal("Tags: {0}", String.Join(", ", metaInfo.Tags));
        }
    }

    public Uri Url { get; }

    public string Title
    {
        get { return title; }
        private set { if (String.IsNullOrEmpty(title)) { title = value; } }
    }

    public string Description
    {
        get { return description; }
        private set { if (String.IsNullOrEmpty(description)) { description = value; } }
    }

    public Uri ImageUrl
    {
        get { return imageUrl; }
        private set { if (imageUrl == null) { imageUrl = value; } }
    }

    public string Author
    {
        get { return author; }
        private set { if (String.IsNullOrEmpty(author)) { author = value; } }
    }

    public string SiteName
    {
        get { return siteName; }
        private set { if (String.IsNullOrEmpty(siteName)) { siteName = value; } }
    }

    public string Type
    {
        get { return type; }
        private set { if (String.IsNullOrEmpty(type)) { type = value; } }
    }

    public IReadOnlyCollection<string> Tags
    {
        get { return tags; }
    }

    public static SiteMetaInformation FromUrl(Uri url)
    {
        var metaInfo = new SiteMetaInformation(url);

        var web = new HtmlWeb();
        var document = web.Load(url);
        var metaTags = document.DocumentNode.SelectNodes("//meta");

        if (metaTags == null)
        {
            return metaInfo;
        }

        metaInfo.FromMetaTags(metaTags, metaInfo.ReadOpenGraph);
        metaInfo.FromMetaTags(metaTags, metaInfo.ReadTwitterCard);
        metaInfo.FromMetaTags(metaTags, metaInfo.ReadHtmlMeta);

        var headTitle = document.DocumentNode.SelectSingleNode("/html/head/title");
        metaInfo.FromHtmlElement(headTitle);

        return metaInfo;
    }

    void FromMetaTags(HtmlNodeCollection metaTags, Action<string, string> read)
    {
        foreach (var metaTag in metaTags)
        {
            var tagName = metaTag.Attributes["name"];
            var tagContent = metaTag.Attributes["content"];
            var tagProperty = metaTag.Attributes["property"];

            var key = tagProperty?.Value ?? tagName?.Value;
            var content = tagContent?.Value;

            if (key != null && content != null)
            {
                read(key, content);
            }
        }
    }

    void FromHtmlElement(HtmlNode element)
    {
        if (element == null || element.Name == null || element.InnerText == null)
        {
            return;
        }

        ReadHtmlMeta(element.Name, element.InnerText);
    }

    void ReadOpenGraph(string key, string value)
    {
        switch (key.ToLower())
        {
            case "og:title":
                Title = value;
                break;
            case "og:image":
                ImageUrl = new Uri(value);
                break;
            case "og:author":
                Author = value;
                break;
            case "og:description":
                Description = value;
                break;
            case "og:site_name":
                SiteName = value;
                break;
            case "og:type":
                Type = value;
                break;
            case "article:tag":
                AddTag(value);
                break;
        }
    }

    void ReadTwitterCard(string key, string value)
    {
        switch (key.ToLower())
        {
            case "twitter:title":
                Title = value;
                break;
            case "twitter:image":
                ImageUrl = new Uri(value);
                break;
            case "twitter:description":
                Description = value;
                break;
        }
    }

    void ReadHtmlMeta(string key, string value)
    {
        switch (key.ToLower())
        {
            case "title":
                Title = value;
                break;
            case "author":
                Author = value;
                break;
            case "keywords":
                foreach (var tag in value.Split(",", StringSplitOptions.RemoveEmptyEntries))
                {
                    AddTag(tag);
                }
                break;
            case "description":
                Description = value;
                break;
        }
    }

    void AddTag(string tag)
    {
        if (String.IsNullOrWhiteSpace(tag))
        {
            return;
        }

        tags.Add(tag.Trim());
    }
}
