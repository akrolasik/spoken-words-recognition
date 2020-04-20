using System.Collections.Generic;

namespace NeuralNetwork.API.Config
{
    public class TrainingConfig
    {
        public int WordSetSize { get; set; }
        public int? IterationCount { get; set; }
        public string PackageFileName { get; set; }
        public List<string> WordsIncluded { get; set; }
        public List<string> AccentsIncluded { get; set; }
        public List<string> ModificationsIncluded { get; set; }
    }
}