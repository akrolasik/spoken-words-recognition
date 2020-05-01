namespace NeuralNetwork.API.Config
{
    public class TrainingConfig
    {
        public int WordSetSize { get; set; }
        public float GradientFactor { get; set; }
        public int? MaxCalculationTimeInMinutes { get; set; }
    }
}