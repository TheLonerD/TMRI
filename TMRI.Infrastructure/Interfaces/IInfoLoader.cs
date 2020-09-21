using System.Collections.Generic;
using System.Threading.Tasks;
using TMRI.Primitives.Config;
using TMRI.Primitives.Definitions;

namespace TMRI.Infrastructure.Interfaces
{
    public interface IInfoLoader
    {
        public Task PrepareLoaderAsync();
        public Task<AppConfig> GetConfigAsync();
        public Task<GameDirectories> GetGamesListAsync();
        public Task<List<MusicDefinition>> GetMusicListAsync();
    }
}
