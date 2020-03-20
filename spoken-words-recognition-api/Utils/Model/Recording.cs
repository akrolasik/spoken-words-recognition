using System;
using Utils.Interfaces;

namespace Utils.Model
{
    public class Recording : ITableRow, ITextFile
    {
        public Guid Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public Guid SpeakerId { get; set; }
        public string Accent { get; set; }
        public string Localization { get; set; }
        public Modification Modification { get; set; }
        public string Word { get; set; }

        public string ToText()
        {
            return $"{Id}\t{SpeakerId}\t{Word}\t{Accent}\t{Localization}\t{Modification}";
        }

        public ITextFile FromText(string text)
        {
            var temp = text.Split('\t');

            return new Recording
            {
                Id = Guid.Parse(temp[0]),
                SpeakerId = Guid.Parse(temp[1]),
                Word = temp[2],
                Accent = temp[3],
                Localization = temp[4],
                Modification = (Modification)Enum.Parse(typeof(Modification), temp[5])
            };
        }
    }
}