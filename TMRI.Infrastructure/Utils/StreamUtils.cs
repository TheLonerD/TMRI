﻿using System;
using System.IO;
using System.Threading.Tasks;

namespace TMRI.Infrastructure.Utils
{
    public class StreamUtils
    {
        public static async Task<MemoryStream> CopyStreamAsync(Stream source, int offset, int length)
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

                bytesToRead = bytesToRead > buffer.Length ? buffer.Length : bytesToRead;

                var read = await source.ReadAsync(buffer, 0, bytesToRead);

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
