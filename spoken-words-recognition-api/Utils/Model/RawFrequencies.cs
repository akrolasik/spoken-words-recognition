using System;
using System.Linq;
using Utils.Interfaces;

namespace Utils.Model
{
    public class RawFrequencies : ITextFile
    {
        private const string ElementSplitter = "---element---";
        private const string PropertySplitter = "---property---";

        public FrequenciesChunk[] FrequenciesChunks { get; set; }

        public string ToText()
        {
            return string.Join(ElementSplitter, FrequenciesChunks.Select(x => $"{x.Milliseconds}{PropertySplitter}{x.Data}"));
        }

        public ITextFile FromText(string text)
        {
            var lines = text.Split(new[] { ElementSplitter }, StringSplitOptions.None);

            return new RawFrequencies
            {
                FrequenciesChunks = lines.Select(x =>
                {
                    var temp = x.Split(new[] { PropertySplitter }, StringSplitOptions.None);
                    return new FrequenciesChunk
                    {
                        Milliseconds = float.Parse(temp.First()),
                        Data = temp.Last()
                    };
                }).ToArray()
            };
        }
    }
}