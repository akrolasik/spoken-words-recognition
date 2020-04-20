using System.Collections.Generic;
using System.Linq;
using NeuralNetwork.API.Config;

namespace NeuralNetwork.API.Statistics
{
    public class NeuralNetworkStatistics
    {
        public NeuralNetworkStatistics(EvolutionConfig evolutionConfig)
        {
            var count = evolutionConfig.NetworkConfig.OutputCount;
            Output = Enumerable.Range(0, count).Select(x => 
                Enumerable.Range(0, count).Select(y => new Aggregation()).ToArray())
                .ToList();
        }

        public int CurrentIteration { get; set; }
        public double TotalComputingTimeInSeconds { get; set; }
        public List<Aggregation[]> Output { get; set; }
        public List<double> Cost { get; set; } = new List<double>();
    }
}