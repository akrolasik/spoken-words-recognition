namespace NeuralNetwork.API.Config
{
    public class GradientConfig
    {
        public float GradientFactor { get; set; }
        public float WeightGradientFactor { get; set; }
        public float BiasGradientFactor { get; set; }
        public float InputGradientFactor { get; set; }
    }
}