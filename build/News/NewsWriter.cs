using System;
using System.IO;
using System.Text;
using System.Globalization;
using Nuke.Common.IO;
using static Nuke.Common.Logger;

namespace DotNetRu.Site.News
{
    public sealed class NewsWriter
    {
        AbsolutePath newsDir;
        string publisherName;

        public NewsWriter(AbsolutePath newsDir, string publisherName)
        {
            this.newsDir = newsDir;
            this.publisherName = publisherName;
        }

        public void BootstrapUrl(Uri url)
        {
            var metaInfo = SiteMetaInformation.FromUrl(url);

            var (date, newsFile) = FindFreeSlot();

            var content = FormatNews(date, metaInfo);

            Info($"Create news file: {Path.GetFileName(newsFile)}");

            var uft8 = new UTF8Encoding(false);
            File.WriteAllText(newsFile, content, uft8);
        }

        (DateTime, AbsolutePath) FindFreeSlot()
        {
            var date = DateTime.UtcNow;
            AbsolutePath newsFile;

            var tries = 0;
            do
            {
                if (tries > 0)
                {
                    date = date.AddMinutes(1);
                }

                var fileName = date.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture) + ".md";
                newsFile = (AbsolutePath) Path.Combine(newsDir, fileName);

            } while (File.Exists(newsFile));

            return (date, newsFile);
        }

        string FormatNews(DateTime date, SiteMetaInformation metaInfo)
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
}
