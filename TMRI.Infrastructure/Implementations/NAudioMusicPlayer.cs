using System.IO;
using System.Threading.Tasks;
using TMRI.Infrastructure.Interfaces;

namespace TMRI.Infrastructure.Implementations
{
    public class NAudioMusicPlayer : IMusicPlayer
    {
        public async Task Play(Stream stream) => throw new System.NotImplementedException();

        public async Task Play(string path) => throw new System.NotImplementedException();
    }
}
