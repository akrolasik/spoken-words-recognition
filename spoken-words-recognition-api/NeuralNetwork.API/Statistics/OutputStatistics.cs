using System.Collections.Generic;
using System.Linq;
using NeuralNetwork.API.Data;

namespace NeuralNetwork.API.Statistics
{
    public class OutputStatistics
    {
        public Dictionary<string, Output> Words { get; set; } = new Dictionary<string, Output>();

        private List<Output> _lastOutput = new List<Output>();

        public void Update(Network.NeuralNetwork neuralNetwork, List<TrainingData> trainingData, int iterationCount)
        {
            foreach (var output in _lastOutput)
            {
                output.IterationCount += iterationCount;
            }

            _lastOutput = new List<Output>();

            for (var i = 0; i < trainingData.Count; i++)
            {
                if (!Words.ContainsKey(trainingData[i].Recording.Id.ToString()))
                {
                    Words[trainingData[i].Recording.Id.ToString()] = new Output();
                }

                Words[trainingData[i].Recording.Id.ToString()].SetValues(trainingData[i], neuralNetwork.Output.Last().GetElement(0, i).ToColumnMajorArray());

                _lastOutput.Add(Words[trainingData[i].Recording.Id.ToString()]);
            }
        }
    }
}