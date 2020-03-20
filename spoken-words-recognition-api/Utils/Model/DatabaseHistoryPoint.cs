using System;
using Utils.Interfaces;

namespace Utils.Model
{
    public class DatabaseHistoryPoint : ITableRow
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTimeOffset Timestamp { get; set; }

        public TimeSpan PackingTime;
        public TimeSpan UploadTime;
        public float PackageSizeInKb;
        public float FilesSizeInKb;
        public int NumberOfSpeakers;
        public int NumberOfRecordings;
    }
}
