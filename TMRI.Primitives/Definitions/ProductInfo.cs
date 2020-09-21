using System.Text.Json.Serialization;

namespace TMRI.Primitives.Definitions
{
    public class ProductInfo
    {
        public LocalizedString Name { get; set; }
        public int Year { get; set; }
        public string Artist { get; set; }
        public LocalizedString Circle { get; set; }
        public int Tracks { get; set; }

        public PackInfo PackInfo { get; set; }

        [JsonIgnore]
        public string Icon { get; set; }
    }
}
