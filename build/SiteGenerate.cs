using System.Threading.Tasks;
using Statiq.App;

public class SiteGenerate
{
    public static Task<int> Main(string[] args) => Bootstrapper
        .Factory
        .CreateDefault(args)
        .RunAsync();
}
