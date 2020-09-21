using System.Collections.Generic;
using TMRI.Primitives.Config;
using TMRI.Primitives.Definitions;

namespace TMRI.Infrastructure
{
    public class TMRIStorage
    {
        public AppConfig Config { get; set; }
        public GameDirectories GamesDir { get; set; }
        public List<MusicDefinition> Products { get; set; }
    }
}
