using System;
using System.Linq;
using Utils.Interfaces;

namespace Utils.UnitTests.Helpers
{
    public class ExampleTextFile : ITextFile
    {
        public string String { get; set; }
        public int Integer { get; set; }
        public bool Boolean { get; set; }
        public float Float { get; set; }
        public Guid Guid { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public ExampleEnum Enum { get; set; }

        public string ToText()
        {
            var properties = GetType().GetProperties().ToList();
            return string.Join("\n", properties.Select(x => x.GetValue(this)));
        }

        public ITextFile FromText(string text)
        {
            var values = text.Split('\n').ToList();
            var index = 0;

            return new ExampleTextFile
            {
                String = values[index++],
                Integer = int.Parse(values[index++]),
                Boolean = bool.Parse(values[index++]),
                Float = float.Parse(values[index++]),
                Guid = Guid.Parse(values[index++]),
                DateTimeOffset = DateTimeOffset.Parse(values[index++]),
                TimeSpan = TimeSpan.Parse(values[index++]),
                Enum = (ExampleEnum)System.Enum.Parse(typeof(ExampleEnum), values[index])
            };
        }
    }
}