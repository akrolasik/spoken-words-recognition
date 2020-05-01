using NeuralNetwork.API.Network;

namespace NeuralNetwork.API.Statistics
{
    public class LayerStatistics
    {
        public ValuesDistribution Weight { get; set; }
        public ValuesDistribution Bias { get; set; }
        public ValuesDistribution Output { get; set; }
    }
}