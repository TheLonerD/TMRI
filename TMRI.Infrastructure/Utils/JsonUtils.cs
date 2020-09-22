using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using TMRI.Primitives.Converters;

namespace TMRI.Infrastructure.Utils
{
    public static class JsonUtils
    {
        public static async Task SerializeAsync(Stream stream, object obj)
        {
            await JsonSerializer.SerializeAsync(stream, obj, obj.GetType(), new JsonSerializerOptions
            {
                Converters = {new JsonStringConverter()},
                IgnoreNullValues = true, // Lesser properties == easier to read
                WriteIndented = true, // Write formatted JSON
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // Do not escape unicode sequence
                PropertyNameCaseInsensitive = true, // PrOpErTy == property
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase // "TestVarOkYeah"
            });
        }

        public static async Task<T> DeserializeAsync<T>(Stream stream)
        {
            var result = await JsonSerializer.DeserializeAsync<T>(stream, new JsonSerializerOptions
            {
                Converters = {new JsonStringConverter()},
                IgnoreNullValues = true,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return result;
        }
    }
}
