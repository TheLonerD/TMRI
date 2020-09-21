using System.Collections.Generic;

namespace TMRI.Primitives.Definitions
{
    public class TrackInfo
    {
        public long Number { get; set; }
        public LocalizedString Name { get; set; }
        public List<LocalizedString> Comments { get; set; }
        public MetaInfo Meta { get; set; }
    }
}
