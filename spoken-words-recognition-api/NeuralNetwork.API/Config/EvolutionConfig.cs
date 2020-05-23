using System;
using Newtonsoft.Json;

namespace NeuralNetwork.API.Config
{
    public class EvolutionConfig
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        [JsonIgnore]
        public EvolutionState State { get; set; } = EvolutionState.Idle;

        public NetworkConfig NetworkConfig { get; set; }
        public InputConfig InputConfig { get; set; }
    }
}