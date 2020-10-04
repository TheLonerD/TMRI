using System;
using System.IO;
using System.Threading.Tasks;
using TMRI.Primitives;

namespace TMRI.Infrastructure.Interfaces
{
    public interface IMusicPlayer : IDisposable
    {
        EventHandler PlaybackStopped { get; set; }
        PlayInfo PlayInfo { get; set; }

        Task LoadFileAsync(Stream stream);
        Task LoadFileAsync(string path);
        void Play();
        void Pause();
        void Stop();
    }
}
