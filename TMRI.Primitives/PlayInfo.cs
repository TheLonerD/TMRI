namespace TMRI.Primitives
{
    public class PlayInfo
    {
        public long Start { get; set; }
        public long Loop { get; set; }
        public long End { get; set; }
        public long Length => End - Start;
    }
}
