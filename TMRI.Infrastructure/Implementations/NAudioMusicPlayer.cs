using System;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;
using TMRI.Infrastructure.Interfaces;
using TMRI.Primitives;
using TMRI.Primitives.Enums;

namespace TMRI.Infrastructure.Implementations
{
    public class NAudioMusicPlayer : IMusicPlayer
    {
        public EventHandler PlaybackStopped { get; set; }
        public PlayInfo PlayInfo { get; set; }

        private WaveOutEvent _woEvent;
        private MusicPlayerState _state = MusicPlayerState.Stopped;

        public NAudioMusicPlayer()
        {
            _woEvent = new WaveOutEvent();
            _woEvent.PlaybackStopped += (sender, e) =>
            {
                _state = MusicPlayerState.Stopped;
                PlaybackStopped?.Invoke(this, EventArgs.Empty);
            };
        }

        public MusicPlayerState State => _state;

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
            _woEvent.Play();
            _state = MusicPlayerState.Played;
        }

        public void Pause()
        {
            _woEvent.Pause();
            _state = MusicPlayerState.Paused;
        }

        public void Stop()
        {
            _woEvent.Stop();
            _state = MusicPlayerState.Stopped;
        }

        public void Dispose()
        {
            
            _woEvent?.Dispose();
            _woEvent = null;
        }
    }
}
