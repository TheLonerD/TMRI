using System.Collections.Generic;
using System.Text.Json.Serialization;
using TMRI.Primitives.Converters;
using TMRI.Primitives.Enums;

namespace TMRI.Primitives
{
    [JsonConverter(typeof(LocalizedStringConverter))]
    public class LocalizedString: Dictionary<Language, string>, IDictionary<Language, string>
    {
        public new string this[Language key]
        {
            get => TryGetValue(key, out var value) ? value : base[Language.JP];
            set
            {
                if (ContainsKey(key))
                {
                    base[key] = value;
                }
                else
                {
                    Add(key, value);
                }
            }
        }
    }
}
