using System.Collections.Generic;

namespace DotNetRu.Site.Utils
{
    static class New
    {
        public static IEnumerable<KeyValuePair<string, object>> Metadata(string key, object value) =>
            new[] { new KeyValuePair<string, object>(key, value) };
    }
}
