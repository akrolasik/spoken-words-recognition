using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagedCuda;
using ManagedCuda.BasicTypes;
using MathNet.Numerics.LinearAlgebra;
using NeuralNetwork.API.Config;
using NeuralNetwork.API.Data;
using NeuralNetwork.API.Extensions;
using NeuralNetwork.API.Statistics;
using Newtonsoft.Json;

namespace NeuralNetwork.API.Network
{
    public class Evolution
    {
        public readonly EvolutionConfig Config;
        public readonly NeuralNetwork Network;
        public readonly NeuralNetworkStatistics Statistics;

        private readonly DataProvider _dataProvider;

        private CancellationTokenSource _cancellationTokenSource;
        private NeuralNetworkResult _lastResult;
        private DateTime _lastSavingTime = DateTime.UtcNow;

        private string BasePath => $"temp/evolutions/{Config.Id}";

        public Evolution(EvolutionConfig evolutionConfig)
        {
            Config = evolutionConfig;
            Network = LoadNetwork();
            Statistics = LoadStatistics();
            Config.IsRunning = false;

            _dataProvider = new DataProvider(evolutionConfig);
        }

        private NeuralNetwork LoadNetwork()
        {
            if (!File.Exists($"{BasePath}/layer{0}.bin"))
            {
                return new NeuralNetwork(Config);
            }

            var layers = new List<MatrixFunction>();

            for (var l = 0; l < Config.NetworkConfig.HiddenLayersNeuronCount.Length + 1; l++)
            {
                var weightWidth = Config.NetworkConfig.GetLayerWeightWidth(l);
                var weightHeight = Config.NetworkConfig.GetLayerWeightHeight(l);
                var weightParamCount = weightWidth * weightHeight;

                using var fileStream = new FileStream($"{BasePath}/layer{l}.bin", FileMode.Open);
                using var stream = new BinaryReader(fileStream);
                var weightBytes = stream.ReadBytes(weightParamCount * 8);
                var biasBytes = stream.ReadBytes(weightHeight * 8);

                var weight = Enumerable.Range(0, weightParamCount).Select(x => BitConverter.ToDouble(weightBytes, x * 8)).ToArray();
                var bias = Enumerable.Range(0, weightHeight).Select(x => BitConverter.ToDouble(biasBytes, x * 8)).ToArray();

                var weightMatrix = Matrix<double>.Build.Dense(weightHeight, weightWidth, weight);
                var biasMatrix = Matrix<double>.Build.Dense(weightHeight, 1, bias);

                layers.Add(new MatrixFunction(weightMatrix, biasMatrix));
            }

            return new NeuralNetwork(Config, layers);
        }

        private NeuralNetworkStatistics LoadStatistics()
        {
            var path = $"{BasePath}/statistics.json";

            if (!File.Exists(path))
            {
                return new NeuralNetworkStatistics(Config);
            }

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<NeuralNetworkStatistics>(json);
        }

        public static void Remove(EvolutionConfig config)
        {
            var path = $"temp/evolutions/{config.Id}";

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        private void Save(bool force = false)
        {
            if (force || _lastSavingTime + TimeSpan.FromMinutes(5) < DateTime.UtcNow)
            {
                Directory.CreateDirectory(BasePath);
                SaveNetwork();
                SaveStatistics();

                _lastSavingTime = DateTime.UtcNow;
            }
        }

        private void SaveStatistics()
        {
            var json = JsonConvert.SerializeObject(Statistics);
            File.WriteAllText($"{BasePath}/statistics.json", json);
        }

        private void SaveNetwork()
        {
            for (var l = 0; l < Network.Layers.Count; l++)
            {
                var weightArray = Network.Layers[l].Weight.ToColumnMajorArray();
                var biasArray = Network.Layers[l].Bias.ToColumnMajorArray();
                var byteArray = weightArray.SelectMany(BitConverter.GetBytes).ToArray().ToList();
                byteArray.AddRange(biasArray.SelectMany(BitConverter.GetBytes).ToArray());

                using var fileStream = new FileStream($"{BasePath}/layer{l}.bin", FileMode.Create);
                using var stream = new BinaryWriter(fileStream);
                stream.Write(byteArray.ToArray());
            }
        }

        public async Task StartCalculation()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            Config.IsRunning = true;

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (Statistics.CurrentIteration > 0)
                {
                    ApplyGradient();
                }

                var dataSet = _dataProvider.GetRandomDataSet();
                await Calculate(dataSet);
                Save();
            }

            Save(true);

            Config.IsRunning = false;
        }

        private async Task Calculate(List<TrainingData> dataSet)
        {
            const int taskCount = 10;
            var wordsPerTask = dataSet.Count / taskCount;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var tasks = Enumerable.Range(0, taskCount).Select(x => Task.Run(() => dataSet
                .Skip(x * wordsPerTask).Take(wordsPerTask).ToList()
                .Select(Network.Calculate).ToList())).ToArray();

            await Task.WhenAll(tasks);
            var results = tasks.SelectMany(x => x.Result).ToList();

            UpdateStatistics(dataSet, results, stopwatch);
        }

        public void CancelCalculation()
        {
            _cancellationTokenSource.Cancel();
        }

        private void ApplyGradient()
        {
            for (var i = 0; i < Network.Layers.Count; i++)
            {
                Network.Layers[i].Weight += _lastResult.Gradient.Layers[i].Weight * Config.GradientConfig.GradientFactor;
                Network.Layers[i].Bias += _lastResult.Gradient.Layers[i].Bias * Config.GradientConfig.GradientFactor;
            }
        }

        private void UpdateStatistics(List<TrainingData> dataSet, List<NeuralNetworkResult> results, Stopwatch stopwatch)
        {
            _lastResult = results.Average();

            for (var setIndex = 0; setIndex < dataSet.Count; setIndex++)
            {
                var outputStats = Statistics.Output[dataSet[setIndex].OutputIndex];

                for (var outputIndex = 0; outputIndex < outputStats.Length; outputIndex++)
                {
                    outputStats[outputIndex].Min =
                        Math.Min(outputStats[outputIndex].Min, results[setIndex].Output[outputIndex]);

                    outputStats[outputIndex].Max =
                        Math.Min(outputStats[outputIndex].Max, results[setIndex].Output[outputIndex]);

                    outputStats[outputIndex].Avg =
                        (outputStats[outputIndex].Avg * Statistics.Cost.Count + results[setIndex].Output[outputIndex]) /
                        (Statistics.Cost.Count + 1);
                }
            }

            
            Statistics.CurrentIteration++;

            if (Math.Abs(Math.Pow((Statistics.CurrentIteration + 1), 1.0 / (Statistics.Cost.Count + 1)) - 2.0) < 0.0001)
            {
                Statistics.Cost.Add(_lastResult.Cost);
            }

            //Statistics.TotalComputingTimeInSeconds += (double)stopwatch.Elapsed.TotalSeconds;
        }
    }
}