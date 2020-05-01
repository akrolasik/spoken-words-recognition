using System.Collections.Generic;
using System.Linq;

namespace NeuralNetwork.API.Statistics
{
    public class ValuesDistribution
    {
        public float Min { get; set; }
        public float Max { get; set; }
        public List<int> Buckets { get; set; } = new List<int>();

        public ValuesDistribution()
        {

        }

        public ValuesDistribution(float[] set)
        {
            Min = set.Min();
            Max = set.Max();

            const int bucketsCount = 25;
            var width = (Max - Min) / bucketsCount;

            for (var i = 0; i < bucketsCount; i++)
            {
                var from = i * width;
                var to = from + width;

                Buckets.Add(set.Count(x => x >= from && x <= to));
            }
        }
    }
}