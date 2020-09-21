using System;
using System.IO;
using System.Threading.Tasks;

namespace TMRI.Infrastructure.Utils
{
    public class StreamUtils
    {
        public static async Task<MemoryStream> CopyStreamAsync(Stream source, int length)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var ms = new MemoryStream();
            var buffer = new byte[64 * 1024]; // Use 64 KB buffer
            int total = 0;

            while (total != length)
            {
                var bytesToRead = length - total;

                if (bytesToRead <= 0)
                {
                    bytesToRead = length;
                }

                bytesToRead = bytesToRead > buffer.Length ? buffer.Length : bytesToRead;

                var read = await source.ReadAsync(buffer, 0, bytesToRead);

                if (read == 0)
                {
                    break;
                }

                total += read;
            }

            return ms;
        }
    }
}
