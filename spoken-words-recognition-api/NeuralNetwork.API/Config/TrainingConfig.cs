namespace NeuralNetwork.API.Config
{
    public class TrainingConfig
    {
        public bool UseGpu { get; set; }
        public int WordSetSize { get; set; }
        public int CalculationThreadCount { get; set; }
        public int SavingThreadCount { get; set; }
        public int PopulationSize { get; set; }
        public int? IterationCount { get; set; }
    }
}