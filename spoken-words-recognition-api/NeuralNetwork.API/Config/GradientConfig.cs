namespace NeuralNetwork.API.Config
{
    public class GradientConfig
    {
        public double GradientFactor { get; set; }
        public double WeightGradientFactor { get; set; }
        public double BiasGradientFactor { get; set; }
        public double InputGradientFactor { get; set; }
    }
}