using TMRI.Primitives.Enums;

namespace TMRI.Primitives.Definitions
{
    public class PackInfo
    {
        public PackMethod PackMethod { get; set; } = PackMethod.None;
        public string BGMFile { get; set; }
        public string BGMDir { get; set; }
        public MetaInfo MetaInfo { get; set; }
    }
}
