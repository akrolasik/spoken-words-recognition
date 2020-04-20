using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NeuralNetwork.API.Config;
using NeuralNetwork.API.Network;
using NeuralNetwork.API.Statistics;
using Newtonsoft.Json;

namespace NeuralNetwork.API.Repository
{
    public class EvolutionRepository : IEvolutionRepository
    {
        private const string EvolutionFileName = "temp/evolutions.json";
        private readonly List<EvolutionConfig> _evolutions;
        private Evolution _currentEvolution;

        public EvolutionRepository()
        {
            _evolutions = new List<EvolutionConfig>();

            if (!Directory.Exists("temp"))
            {
                Directory.CreateDirectory("temp");
            }

            if (File.Exists(EvolutionFileName))
            {
                var json = File.ReadAllText(EvolutionFileName);
                _evolutions = JsonConvert.DeserializeObject<List<EvolutionConfig>>(json);
            }
        }

        public void AddEvolution(EvolutionConfig evolutionConfig)
        {
            _evolutions.Add(evolutionConfig);

            SaveEvolutions();

            _ = Task.Run(() => new Evolution(evolutionConfig));
        }

        public void DeleteEvolution(Guid id)
        {
            StopRunningEvolution();

            var config = _evolutions.First(x => x.Id == id);
            Evolution.Remove(config);

            _evolutions.Remove(config);
            SaveEvolutions();
        }

        public List<EvolutionConfig> GetEvolutions()
        {
            return _evolutions;
        }

        public async Task StartEvolution(Guid id)
        {
            StopRunningEvolution();

            var config = _evolutions.First(x => x.Id == id);
            _currentEvolution = new Evolution(config);

            while (true)
            {
                await _currentEvolution.StartCalculation();
            }
        }

        public void StopRunningEvolution()
        {
            if (_currentEvolution != null)
            {
                _currentEvolution.CancelCalculation();

                while (!_currentEvolution.Config.IsRunning)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }

                _currentEvolution = null;
            }
        }

        public NeuralNetworkStatistics GetNeuralNetworkStatistics(Guid id)
        {
            try
            {
                return _currentEvolution.Statistics;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private void SaveEvolutions()
        {
            var json = JsonConvert.SerializeObject(_evolutions);
            File.WriteAllText(EvolutionFileName, json);
        }
    }
}