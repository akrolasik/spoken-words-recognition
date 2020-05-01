using System;
using System.Collections.Generic;
using System.Linq;
using NeuralNetwork.API.Data;

namespace NeuralNetwork.API.Statistics
{
    public class OutputStatistics
    {
        public Dictionary<string, Output> Words { get; set; } = new Dictionary<string, Output>();

        public void Update(Network.NeuralNetwork neuralNetwork, List<TrainingData> trainingData)
        {
            try
            {
                for (var i = 0; i < trainingData.Count; i++)
                {
                    if (!Words.ContainsKey(trainingData[i].Recording.Id.ToString()))
                    {
                        Words[trainingData[i].Recording.Id.ToString()] = new Output();
                    }

                    Words[trainingData[i].Recording.Id.ToString()].SetValues(trainingData[i], neuralNetwork.Output.Last().GetElement(0, i).ToColumnMajorArray());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}