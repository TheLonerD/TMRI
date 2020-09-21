using NAudio.Wave;

namespace TMRI.Primitives
{
    public class LoopStream : WaveStream
    {
        // https://markheath.net/post/looped-playback-in-net-with-naudio

        private readonly WaveStream _sourceStream;
        private long _loopCount = 0;

        public long LoopPosition { get; set; } = 0;
        public long LoopCount { get; set; } = 2;

        public LoopStream(WaveStream sourceStream)
        {
            _sourceStream = sourceStream;
        }

        public override WaveFormat WaveFormat => _sourceStream.WaveFormat;
        public override long Length => _sourceStream.Length + ((_sourceStream.Length - LoopPosition) * LoopCount);

        public override long Position
        {
            get => _sourceStream.Position + (_sourceStream.Length * _loopCount);
            set => _sourceStream.Position = value - (_sourceStream.Length * _loopCount);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                var bytesRead = _sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    if (_loopCount == LoopCount)
                    {
                        return 0;
                    }
                    
                    // loop
                    _sourceStream.Position = LoopPosition;
                    _loopCount++;
                }

                totalBytesRead += bytesRead;
            }
            
            return totalBytesRead;
        }
    }
}
