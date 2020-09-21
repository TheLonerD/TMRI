using System.Collections.Generic;
using System.Text.Json.Serialization;
using TMRI.Primitives.Converters;

namespace TMRI.Primitives
{
    [JsonConverter(typeof(LocalizedStringConverter))]
    public class LocalizedString: Dictionary<Language, string>
    {
    }
}
