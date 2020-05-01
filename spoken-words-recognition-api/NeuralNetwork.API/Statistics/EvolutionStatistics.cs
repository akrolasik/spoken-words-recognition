using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NeuralNetwork.API.Data;

namespace NeuralNetwork.API.Statistics
{
    public class EvolutionStatistics
    {
        public int IterationCount { get; set; }
        public float TotalComputingTimeInSeconds { get; set; }
        public OutputStatistics Output{ get; set; } = new OutputStatistics();
        public List<Aggregation> CostHistory { get; set; } = new List<Aggregation>();

        private float _cachedTime;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public void StartMeasuringTime()
        {
            _cachedTime = TotalComputingTimeInSeconds;
            _stopwatch.Restart();
        }

        public void Update(Network.NeuralNetwork neuralNetwork, List<TrainingData> trainingData)
        {
            IterationCount++;
            TotalComputingTimeInSeconds = _cachedTime + (float)_stopwatch.Elapsed.TotalSeconds;

            var updateCostHistory = Math.Abs(Math.Log2(IterationCount) % 1) < 1.0E-5f;

            if (updateCostHistory)
            {
                Output.Update(neuralNetwork, trainingData);

                var cost = Output.Words.Select(x => x.Value.Cost).ToList();

                CostHistory.Add(new Aggregation
                {
                    Avg = cost.Average(),
                    Max = cost.Max(),
                    Min = cost.Min(),
                });
            }
        }
    }
}