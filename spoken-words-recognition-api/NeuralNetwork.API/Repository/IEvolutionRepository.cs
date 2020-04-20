using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeuralNetwork.API.Config;
using NeuralNetwork.API.Statistics;

namespace NeuralNetwork.API.Repository
{
    public interface IEvolutionRepository
    {
        void AddEvolution(EvolutionConfig evolutionConfig);
        void DeleteEvolution(Guid id);
        List<EvolutionConfig> GetEvolutions();
        Task StartEvolution(Guid id);
        void StopRunningEvolution();
        NeuralNetworkStatistics GetNeuralNetworkStatistics(Guid id);
    }
}