using System;

namespace DotNetRu.Site.Radio
{
    public sealed class Episode
    {
        public Episode(string number, string title, DateTime publishDate)
        {
            Number = number;
            Title = title;
            PublishDate = publishDate;
        }

        public string Number { get; }
        public string Title { get; }
        public DateTime PublishDate { get; }
    }
}