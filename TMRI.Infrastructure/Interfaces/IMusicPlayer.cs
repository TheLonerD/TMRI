using System.IO;
using System.Threading.Tasks;

namespace TMRI.Infrastructure.Interfaces
{
    public interface IMusicPlayer
    {
        Task Play(Stream stream);
        Task Play(string path);
    }
}
