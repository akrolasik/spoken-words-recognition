using System;
using Utils.Interfaces;

namespace Utils.Model
{
    public class RawRecording : ITableRow
    {
        public Guid Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public Guid SpeakerId { get; set; }
        public string Accent { get; set; }
        public string Localization { get; set; }
        public Modification Modification { get; set; }

        public WordOccurence[] Words { get; set; }
    }
}