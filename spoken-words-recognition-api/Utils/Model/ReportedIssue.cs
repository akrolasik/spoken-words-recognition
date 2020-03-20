using System;
using Utils.Interfaces;

namespace Utils.Model
{
    public class ReportedIssue : ITableRow
    {
        public Guid Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public Guid SpeakerId { get; set; }
        public string Message { get; set; }
        public string Browser { get; set; }
        public string UserAgent { get; set; }
        public string Localization { get; set; }
        public bool IsImageAttached { get; set; }
    }
}