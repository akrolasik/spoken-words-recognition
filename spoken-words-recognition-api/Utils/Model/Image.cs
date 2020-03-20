using Utils.Interfaces;

namespace Utils.Model
{
    public class Image : ITextFile
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public string Data { get; set; }

        public string ToText()
        {
            return $"{Height}\t{Width}\t{Data}";
        }

        public ITextFile FromText(string text)
        {
            var temp = text.Split('\t');

            return new Image
            {
                Height = int.Parse(temp[0]),
                Width = int.Parse(temp[1]),
                Data = temp[2]
            };
        }
    }
}