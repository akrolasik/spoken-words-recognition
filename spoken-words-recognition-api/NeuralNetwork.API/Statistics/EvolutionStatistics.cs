using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NeuralNetwork.API.Data;

namespace NeuralNetwork.API.Statistics
{
    public class EvolutionStatistics
    {
        private int _lastIterationCount { get; set; }

        public int IterationCount { get; set; }
        public float TotalComputingTimeInSeconds { get; set; }
        public OutputStatistics Output{ get; set; } = new OutputStatistics();
        public List<Aggregation> CostHistory { get; set; } = new List<Aggregation>();
        public List<LayerStatistics> Layers { get; set; } = new List<LayerStatistics>();

        private float _cachedTime;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public void StartMeasuringTime()
        {
            _cachedTime = TotalComputingTimeInSeconds;
            _stopwatch.Restart();
            _lastIterationCount = IterationCount;
        }

        public void Update(Network.NeuralNetwork neuralNetwork, List<TrainingData> trainingData, bool updateWordsOutput)
        {
            IterationCount++;
            TotalComputingTimeInSeconds = _cachedTime + (float)_stopwatch.Elapsed.TotalSeconds;

            var updateCostHistory = Math.Abs(Math.Log2(IterationCount) % 1) < 1.0E-5f;

            if (updateWordsOutput || updateCostHistory)
            {
                Output.Update(neuralNetwork, trainingData, IterationCount - _lastIterationCount);
                _lastIterationCount = IterationCount;
            }

            if (updateCostHistory)
            {
                var cost = Output.Words.Select(x => x.Value.Cost).ToList();

                CostHistory.Add(new Aggregation
                {
                    Avg = cost.Average(),
                    Max = cost.Max(),
                    Min = cost.Min(),
                });
            }

            if (IterationCount % 5000 == 0)
            {
                Layers = Enumerable.Range(0, neuralNetwork.Layers.Count).Select(i => new LayerStatistics
                {
                    Weight = new ValuesDistribution(neuralNetwork.Layers[i].CudaWeight),
                    Bias = new ValuesDistribution(neuralNetwork.Layers[i].CudaBias),
                    Output = new ValuesDistribution(neuralNetwork.Output[i].Parameters)
                }).ToList();
            }
        }
    }
}