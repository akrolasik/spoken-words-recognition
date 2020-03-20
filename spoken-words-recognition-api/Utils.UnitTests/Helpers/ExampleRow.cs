using System;
using Utils.Interfaces;

namespace Utils.UnitTests.Helpers
{
    public class ExampleArrayElement
    {
        public string String { get; set; }
        public int Integer { get; set; }
    }

    public class ExampleRow : ITableRow
    {
        public Guid Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public string String { get; set; }
        public int Integer { get; set; }
        public bool Boolean { get; set; }
        public float Float { get; set; }
        public Guid Guid { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public ExampleEnum Enum { get; set; }
        public ExampleArrayElement[] ExampleArray { get; set; }
}
}