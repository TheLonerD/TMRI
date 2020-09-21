using System;
using System.IO;
using System.Threading.Tasks;

namespace TMRI.Infrastructure.Utils
{
    public class StreamUtils
    {
        public static async Task<MemoryStream> CopyStreamAsync(Stream source, long offset, long length)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var ms = new MemoryStream();
            var buffer = new byte[4096]; // Use 4 KB buffer
            int total = 0;
            source.Seek(offset, SeekOrigin.Begin);

            while (total != length)
            {
                var bytesToRead = length - total;

                if (bytesToRead <= 0)
                {
                    bytesToRead = length;
                }

                int chunkSize = bytesToRead > buffer.Length ? buffer.Length : (int) bytesToRead;

                var read = await source.ReadAsync(buffer, 0, chunkSize);

                if (read == 0)
                {
                    break;
                }

                await ms.WriteAsync(buffer, 0, read);
                Array.Clear(buffer, 0, buffer.Length);

                total += read;
            }

            Array.Clear(buffer, 0, buffer.Length);
            ms.Seek(0, SeekOrigin.Begin);

            return ms;
        }
    }
}
