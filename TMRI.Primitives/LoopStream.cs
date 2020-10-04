using NAudio.Wave;

namespace TMRI.Primitives
{
    public class LoopStream : WaveStream
    {
        // https://markheath.net/post/looped-playback-in-net-with-naudio

        private readonly WaveStream _sourceStream;
        private long _loops;
        private long _loopCount = 0;
        private bool _infinite = true;

        public LoopStream(WaveStream sourceStream)
        {
            _sourceStream = sourceStream;
        }

        public long LoopPosition { get; set; } = 0;

        public long LoopCount
        {
            get => _loopCount;
            set
            {
                if (value > 0)
                {
                    _infinite = false;
                }
                
                _loopCount = value;
            }
        }

        public bool Infinite
        {
            get => _infinite;
            set
            {
                if (value)
                {
                    _loopCount = 0;
                }
                
                _infinite = value;
            }
        }

        public override WaveFormat WaveFormat => _sourceStream.WaveFormat;
        public override long Length => _sourceStream.Length + (_sourceStream.Length - LoopPosition) * LoopCount;

        public override long Position
        {
            get => _sourceStream.Position + (_sourceStream.Length - LoopPosition) * _loops;
            set => _sourceStream.Position = Position - value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                var bytesRead = _sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    if (!Infinite && _loops == LoopCount)
                    {
                        return 0;
                    }

                    // loop
                    _sourceStream.Position = LoopPosition;
                    _loops++;
                }

                totalBytesRead += bytesRead;
            }

            return totalBytesRead;
        }
    }
}
