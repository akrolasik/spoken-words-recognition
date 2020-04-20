using System;

namespace NeuralNetwork.API.Config
{
    public class EvolutionConfig
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsRunning { get; set; }

        public NetworkConfig NetworkConfig { get; set; }
        public TrainingConfig TrainingConfig { get; set; }
        public GradientConfig GradientConfig { get; set; }
    }
}