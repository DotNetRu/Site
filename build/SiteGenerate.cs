using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.App;
using Statiq.Common;
using Statiq.Core;
using Statiq.Markdown;
using Statiq.Yaml;

public class SiteGenerate
{
    // TODO:
    //    [x] Find all Radio files
    //    [x] Specify output directory
    //    [x] Convert Markdown to HTML
    //    [x] Convert Yaml to Metadata
    //    [ ] Use Razor with Layout
    //    [ ] Copy static files untouched
    //    [ ] Add Index as output file
    //    [ ] Add each episode as output file
    public static Task<int> Main(string[] args) => Bootstrapper
        .Factory
        .CreateDefault(args)
        .AddSettings(new Dictionary<string, object>
        {
            { Keys.Host, "radio.dotnet.ru" },
            { Keys.LinksUseHttps, true },
            { Keys.LinkLowercase, true }
        })
        .BuildPipeline("Render Markdown", builder => builder
            .WithInputReadFiles("Radio/*/*.md")
            .WithProcessModules(
                new ExtractFrontMatter(new ParseYaml()),
                new RenderMarkdown())
            .WithOutputWriteFiles(".html"))
        .RunAsync();
}
