using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace TMRI.Primitives.Converters
{
    public class JsonStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            if (string.IsNullOrWhiteSpace(value)) // Treat empty values (even with single space char) as null
            {
                return null;
            }

            value = Regex.Unescape(value)
                .Normalize(NormalizationForm.FormKD)
                .Trim();

            return value;
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                return;
            }

            value = Regex.Escape(value)
                .Normalize(NormalizationForm.FormKD)
                .Trim();

            writer.WriteStringValue(value);
        }
    }
}
