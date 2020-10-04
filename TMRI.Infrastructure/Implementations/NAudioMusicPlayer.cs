using System;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;
using TMRI.Infrastructure.Interfaces;
using TMRI.Primitives;

namespace TMRI.Infrastructure.Implementations
{
    public class NAudioMusicPlayer : IMusicPlayer
    {
        public EventHandler PlaybackStopped { get; set; }
        public PlayInfo PlayInfo { get; set; }

        private WaveOutEvent _woEvent;

        public NAudioMusicPlayer()
        {
            _woEvent = new WaveOutEvent();
            _woEvent.PlaybackStopped += (sender, e) =>
            {
                PlaybackStopped?.Invoke(this, EventArgs.Empty);
            };
        }

        public async Task LoadFileAsync(Stream stream)
        {
            await using var wav = new RawSourceWaveStream(stream, new WaveFormat());
            await using var provider = new LoopStream(wav)
            {
                LoopPosition = PlayInfo.Loop - PlayInfo.Start
            };
            provider.Seek(0, SeekOrigin.Begin);
            
            _woEvent.Init(provider);
        }

        public async Task LoadFileAsync(string path)
        {
            if (!File.Exists(path))
            {
                throw new TMRIException($"File \"{path}\" is not exist");
            }

            await using var fs = new FileStream(path, FileMode.Open);
            await LoadFileAsync(fs);
        }

        public void Play()
        {
            if (_woEvent == null)
            {
                throw new TMRIException("Player is not initialized.");
            }

            _woEvent.Play();
        }

        public void Pause()
        {
            if (_woEvent == null)
            {
                throw new TMRIException("Player is not initialized.");
            }

            _woEvent.Pause();
        }

        public void Stop()
        {
            if (_woEvent == null)
            {
                throw new TMRIException("Player is not initialized.");
            }

            _woEvent.Stop();
        }

        public void Dispose()
        {
            
            _woEvent?.Dispose();
            _woEvent = null;
        }
    }
}
