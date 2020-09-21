using System.IO;
using System.Threading.Tasks;
using TMRI.Primitives;
using TMRI.Primitives.Definitions;

namespace TMRI.Infrastructure.Interfaces
{
    public interface IPacker
    {
        public Task<bool> ValidateFileAsync(MusicDefinition md, string file);
        public Task<bool> ValidateFileAsync(MusicDefinition md, Stream stream);
        public Task<Stream> ExtractSongAsync(TrackInfo trackInfo, Stream stream);
        public PlayInfo GetPlayInfo(TrackInfo trackInfo);
    }
}
