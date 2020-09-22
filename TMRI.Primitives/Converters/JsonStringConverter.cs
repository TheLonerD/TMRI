using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace TMRI.Primitives.Converters
{
    public class JsonStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.GetString()?.Trim().Normalize(NormalizationForm.FormKD);

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value != null ? Regex.Unescape(value).Trim().Normalize(NormalizationForm.FormKD) : null);
    }
}
