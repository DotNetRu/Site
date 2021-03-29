using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.WebEncoders;
using Statiq.App;
using Statiq.Common;

namespace DotNetRu.Site.Generator
{
    public sealed class SiteGenerator
    {
        // TODO:
        //    [x] Find all Radio files
        //    [x] Specify output directory
        //    [x] Convert Markdown to HTML
        //    [x] Convert Yaml to Metadata
        //    [ ] Use Razor with Layout
        //    [ ] Copy static files untouched
        //    [x] Add Index as output file
        //    [ ] Add each episode as output file
        //    [ ] Use design
        public static Task<int> Main(string[] args) => Bootstrapper
            .Factory
            .CreateDefault(args)
            .ConfigureServices((services, settings) =>
            {
                services.Configure<WebEncoderOptions>(FixBasicLatinDefault);
            })
            .AddSettings(new Dictionary<string, object>
            {
                { Keys.Host, "radio.dotnet.ru" },
                { Keys.LinksUseHttps, true },
                { Keys.LinkLowercase, true }
            })
            .RunAsync();

        static void FixBasicLatinDefault(WebEncoderOptions options)
        {
            // https://github.com/aspnet/HttpAbstractions/issues/315
            options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
        }
    }
}
