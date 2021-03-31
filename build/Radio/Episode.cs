using System;

namespace DotNetRu.Site.Radio
{
    public sealed class Episode
    {
        public Episode(string number, string title, DateTime publishDate, string? description)
        {
            Number = number;
            Title = title;
            PublishDate = publishDate;
            Description = description;
        }

        public string Number { get; }
        public string Title { get; }
        public DateTime PublishDate { get; }
        public string? Description { get; }
    }
}
