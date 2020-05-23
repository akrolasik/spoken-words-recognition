using System;
using System.Collections.Generic;
using NeuralNetwork.API.Config;
using NeuralNetwork.API.Statistics;

namespace NeuralNetwork.API.Repository
{
    public interface IEvolutionRepository
    {
        void AddEvolution(EvolutionConfig evolutionConfig);
        void DeleteEvolution(Guid id);
        List<EvolutionConfig> GetEvolutions();
        void StartEvolution(Guid id);
        void StopRunningEvolution();
        EvolutionStatistics GetNeuralNetworkStatistics(Guid id);
        void VerifyEvolution(Guid id);
    }
}