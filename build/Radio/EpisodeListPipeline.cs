using System.Collections.Generic;
using System.Linq;
using DotNetRu.Site.Utils;
using JetBrains.Annotations;
using Statiq.Common;
using Statiq.Core;
using Statiq.Razor;
using Statiq.Yaml;

namespace DotNetRu.Site.Radio
{
    [UsedImplicitly]
    public sealed class EpisodeListPipeline : Pipeline
    {
        public EpisodeListPipeline()
        {
            InputModules = new ModuleList
            {
                new ReadFiles("Radio/*/*.md"),
            };

            ProcessModules = new ModuleList
            {
                new ExtractFrontMatter(new ParseYaml()),
                new ReplaceDocuments(Config.FromContext(FoldChildren))
            };

            OutputModules = new ModuleList
            {
                // Load Razor template to IDocument content
                new MergeContent(new ReadFiles("RadioTemplates/EpisodeList.cshtml")),
                new RenderRazor()
                    .WithModel(Config.FromDocument(MapDocumentToEpisodeList)),
                new WriteFiles()
            };
        }

        static IEnumerable<IDocument> FoldChildren(IExecutionContext context)
        {
            var metadata = New.Metadata(Keys.Children, context.Inputs);
            return new[]
            {
                context.CreateDocument("index.html", metadata)
            };
        }

        static EpisodeList MapDocumentToEpisodeList(IDocument document, IExecutionContext context)
        {
            var episodes = document
                .GetChildren()
                .Select(MapDocumentToEpisode)
                .ToList();

            return new EpisodeList(episodes);
        }

        static Episode MapDocumentToEpisode(IDocument document)
        {
            var number = document.GetString("Number");
            var title = document.GetString("Title");
            var publishDateText = document.GetString("PublishDate");
            var publishDate = DateTimeFormatter.ToDateTime(publishDateText);

            return new Episode(number, title, publishDate);
        }
    }
}
