using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TMRI.Infrastructure.Interfaces;
using TMRI.Infrastructure.Utils;
using TMRI.Primitives;
using TMRI.Primitives.Definitions;

namespace TMRI.Infrastructure.Implementations
{
    public class TSAPacker : IPacker
    {
        public const string HEADER = "ZWAV";

        public async Task<bool> ValidateFileAsync(MusicDefinition md, string file)
        {
            await using var fs = new FileStream(file, FileMode.Open);
            return await ValidateFileAsync(md, fs);
        }

        public async Task<bool> ValidateFileAsync(MusicDefinition md, Stream stream)
        {
            // Validate file header value
            var buffer = new byte[HEADER.Length];
            stream.Position = 0;
            await stream.ReadAsync(buffer, 0, HEADER.Length);

            var fileHeader = Encoding.ASCII.GetString(buffer);

            if (string.IsNullOrWhiteSpace(fileHeader))
            {
                return false;
            }

            if (!fileHeader.Equals(HEADER, StringComparison.Ordinal))
            {
                return false;
            }

            // Validate game number in 0x08 and 0x09 pos
            stream.Position = 0x08;
            var minor = stream.ReadByte();
            var major = stream.ReadByte();

            if (md.Product.PackInfo.MetaInfo?.ContainsKey("ZWAVID_08") != true
                || md.Product.PackInfo.MetaInfo?.ContainsKey("ZWAVID_09") != true)
            {
                throw new TMRIException("Missing ZWAVID_08 or ZWAVID_09 values in PackInfo struct.");
            }

            var zwavid08 = ((JsonElement) md.Product.PackInfo.MetaInfo["ZWAVID_08"]).GetInt32();
            var zwavid09 = ((JsonElement) md.Product.PackInfo.MetaInfo["ZWAVID_09"]).GetInt32();

            if (minor != zwavid08 || major != zwavid09)
            {
                return false;
            }

            return true;
        }

        public async Task<Stream> ExtractSongAsync(TrackInfo trackInfo, Stream stream)
        {
            var pos = ((JsonElement) trackInfo.Meta["position"]).EnumerateArray()
                .Select(j => j.GetInt32())
                .ToList();
            var start = pos[0];
            var end = pos[2] + start;
            var length = end - start;

            // Read file content
            stream.Position = start;
            var ms = await StreamUtils.CopyStreamAsync(stream, length);

            return ms;
        }
    }
}
