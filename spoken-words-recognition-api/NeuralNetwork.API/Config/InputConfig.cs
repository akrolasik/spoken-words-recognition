using System.Collections.Generic;

namespace NeuralNetwork.API.Config
{
    public class InputConfig
    {
        public string PackageFileName { get; set; }
        public List<string> WordsIncluded { get; set; }
        public List<string> AccentsIncluded { get; set; }
        public List<string> ModificationsIncluded { get; set; }
    }
}