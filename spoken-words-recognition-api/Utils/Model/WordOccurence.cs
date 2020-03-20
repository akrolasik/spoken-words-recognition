namespace Utils.Model
{
    public class WordOccurence
    {
        public float Milliseconds { get; set; }
        public Word Word { get; set; }
        public float? Start { get; set; }
        public float? End { get; set; }
    }
}