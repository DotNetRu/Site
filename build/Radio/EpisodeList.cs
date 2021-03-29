using System.Collections.Generic;

namespace DotNetRu.Site.Radio
{
    public sealed class EpisodeList
    {
        public EpisodeList(IReadOnlyList<Episode> episodes)
        {
            Episodes = episodes;
        }

        public IReadOnlyList<Episode> Episodes { get; }
    }
}