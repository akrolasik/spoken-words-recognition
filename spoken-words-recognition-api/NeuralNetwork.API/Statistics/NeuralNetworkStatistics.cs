using System;
using System.Collections.Generic;

namespace NeuralNetwork.API.Statistics
{
    public class NeuralNetworkStatistics
    {
        public NeuralNetworkStatistics()
        {
        }

        //public NeuralNetworkStatistics(EvolutionConfig evolutionConfig)
        //{
        //    var count = evolutionConfig.NetworkConfig.OutputCount;
        //    Output = Enumerable.Range(0, count).Select(x => 
        //        Enumerable.Range(0, count).Select(y => new Aggregation()).ToArray())
        //        .ToList();
        //}

        public Guid UnitId { get; set; }
        public int CurrentIteration { get; set; }
        public float TotalComputingTimeInSeconds { get; set; }
        //public List<Aggregation[]> Output { get; set; }
        public List<float> Cost { get; set; } = new List<float>();
    }
}