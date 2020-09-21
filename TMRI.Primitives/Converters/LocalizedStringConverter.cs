using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TMRI.Primitives.Converters
{
    public class LocalizedStringConverter : JsonConverter<LocalizedString>
    {
        public override LocalizedString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var values = JsonSerializer.Deserialize<Dictionary<string, string>>(ref reader, options);
            var result = new LocalizedString();

            foreach (var (key, value) in values)
            {
                var lang = (Language) Enum.Parse(typeof(Language), key.ToUpper(), true);

                if (Enum.IsDefined(typeof(Language), lang))
                {
                    if (result.ContainsKey(lang))
                    {
                        throw new TMRIException($"Localized String already has value for LANG={lang}.");
                    }

                    result.Add(lang, value);
                }
            }

            return result;
        }

        public override void Write(Utf8JsonWriter writer, LocalizedString value, JsonSerializerOptions options)
        {
            Dictionary<string, string> result = null;

            if (value != null)
            {
                result = value
                    .Select(kv => new KeyValuePair<string, string>(Enum.GetName(typeof(Language), kv.Key)?.ToLower(), kv.Value))
                    .Where(kv => !string.IsNullOrWhiteSpace(kv.Key))
                    .ToDictionary(kv => kv.Key, kv => kv.Value);
            }

            if (result != null)
            {
                JsonSerializer.Serialize(writer, result, options);
            }
        }
    }
}
