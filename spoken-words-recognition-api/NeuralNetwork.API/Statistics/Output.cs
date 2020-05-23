using System;
using System.Linq;
using NeuralNetwork.API.Data;

namespace NeuralNetwork.API.Statistics
{
    public class Output
    {
        public float[] Values { get; set; }
        public int ExpectedOutputIndex { get; set; }
        public float Cost { get; set; }
        public string Word { get; set; }

        public void SetValues(TrainingData trainingData, float[] values)
        {
            ExpectedOutputIndex = trainingData.OutputIndex;
            Values = values;
            Cost = (float)Enumerable.Range(0, Values.Length).Sum(x => Math.Pow(Values[x] - (x == ExpectedOutputIndex ? 1 : 0), 2));
            Word = trainingData.Recording.Word;
        }
    }
}