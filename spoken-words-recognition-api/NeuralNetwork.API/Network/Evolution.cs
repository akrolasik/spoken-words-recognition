using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using NeuralNetwork.API.Config;
using NeuralNetwork.API.Cuda;
using NeuralNetwork.API.Data;
using NeuralNetwork.API.Statistics;
using Newtonsoft.Json;

namespace NeuralNetwork.API.Network
{
    public class Evolution
    {
        public readonly EvolutionConfig Config;
        public readonly NeuralNetwork NeuralNetwork;
        public readonly EvolutionStatistics EvolutionStatistics;

        private DataProvider _dataProvider;
        private CancellationTokenSource _cancellationTokenSource;
        
        private CudaClient _cudaClient;

        private string BasePath => $"temp/evolutions/{Config.Id}";

        public Evolution(EvolutionConfig evolutionConfig)
        {
            Config = evolutionConfig;

            Config.State = EvolutionState.Loading;

            if (Directory.Exists(BasePath))
            {
                NeuralNetwork = LoadNeuralNetwork(BasePath);
                EvolutionStatistics = LoadStatistics(BasePath);
            }
            else
            {
                NeuralNetwork = new NeuralNetwork(Config);
                EvolutionStatistics = new EvolutionStatistics();
            }

            Config.State = EvolutionState.Idle;
        }

        private NeuralNetwork LoadNeuralNetwork(string path)
        {
            var layers = new List<MatrixFunction>();

            for (var l = 0; l < Config.NetworkConfig.HiddenLayersNeuronCount.Length + 1; l++)
            {
                var weightWidth = Config.NetworkConfig.GetLayerWeightWidth(l);
                var weightHeight = Config.NetworkConfig.GetLayerWeightHeight(l);
                var weightParamCount = weightWidth * weightHeight;

                using var fileStream = new FileStream($"{path}/layer{l}.bin", FileMode.Open);
                using var stream = new BinaryReader(fileStream);
                var weightBytes = stream.ReadBytes(weightParamCount * 4);
                var biasBytes = stream.ReadBytes(weightHeight * 4);

                var weight = Enumerable.Range(0, weightParamCount).Select(x => BitConverter.ToSingle(weightBytes, x * 4)).ToArray();
                var bias = Enumerable.Range(0, weightHeight).Select(x => BitConverter.ToSingle(biasBytes, x * 4)).ToArray();

                var weightMatrix = Matrix<float>.Build.Dense(weightHeight, weightWidth, weight);
                var biasMatrix = Matrix<float>.Build.Dense(weightHeight, 1, bias);

                layers.Add(new MatrixFunction(weightMatrix, biasMatrix));
            }

            return new NeuralNetwork(layers);
        }

        private EvolutionStatistics LoadStatistics(string path)
        {
            var json = File.ReadAllText($"{path}/statistics.json");
            return JsonConvert.DeserializeObject<EvolutionStatistics>(json);
        }

        public static void Remove(EvolutionConfig config)
        {
            var basePath = $"temp/evolutions/{config.Id}";
            var backupPath = $"temp/evolutions/{config.Id}_backup";
            var tempPath = $"temp/evolutions/{config.Id}_temp";

            if (Directory.Exists(basePath))
                Directory.Delete(basePath, true);

            if (Directory.Exists(backupPath))
                Directory.Delete(backupPath, true);

            if (Directory.Exists(tempPath))
                Directory.Delete(tempPath, true);
        }

        public async Task Save()
        {
            SaveNeuralNetwork();
            await SaveStatistics();
        }

        private async Task SaveStatistics()
        {
            var json = JsonConvert.SerializeObject(EvolutionStatistics);
            await File.WriteAllTextAsync($"{BasePath}/statistics.json", json);
        }

        private void SaveNeuralNetwork()
        {
            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }

            for (var l = 0; l < NeuralNetwork.Layers.Count; l++)
            {
                var weightArray = NeuralNetwork.Layers[l].Weight.ToColumnMajorArray();
                var biasArray = NeuralNetwork.Layers[l].Bias.ToColumnMajorArray();
                var byteArray = weightArray.SelectMany(BitConverter.GetBytes).ToArray().ToList();
                byteArray.AddRange(biasArray.SelectMany(BitConverter.GetBytes).ToArray());
                using var fileStream = new FileStream($"{BasePath}/layer{l}.bin", FileMode.Create);
                using var stream = new BinaryWriter(fileStream);
                stream.Write(byteArray.ToArray());
            }
        }

        public void Verify()
        {
            Config.State = EvolutionState.Loading;

            _dataProvider = new DataProvider(Config, true);
            _cudaClient = new CudaClient(Config, _dataProvider, true);

            NeuralNetwork.PrepareCuda(_dataProvider, true);

            Config.State = EvolutionState.Running;

            for (var l = 0; l < Config.NetworkConfig.HiddenLayersNeuronCount.Length + 1; l++)
            {
                NeuralNetwork.CalcNeuronValues(_cudaClient, l);
                NeuralNetwork.CudaNeuronValuesStream.Synchronize();
            }

            EvolutionStatistics.Verification.Update(NeuralNetwork, _dataProvider.TrainingData);

            Config.State = EvolutionState.Saving;

            Save().Wait();

            _cudaClient.Dispose();
            NeuralNetwork.Dispose();

            // waiting for the ui to get the data
            Thread.Sleep(TimeSpan.FromSeconds(5));

            Config.State = EvolutionState.Idle;
        }

        public void StartCalculation()
        {
            Config.State = EvolutionState.Loading;

            _cancellationTokenSource = new CancellationTokenSource();

            _dataProvider = new DataProvider(Config);
            _cudaClient = new CudaClient(Config, _dataProvider);

            NeuralNetwork.PrepareCuda(_dataProvider);
            EvolutionStatistics.StartMeasuringTime();

            Config.State = EvolutionState.Running;

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                Calculate();
                EvolutionStatistics.Update(NeuralNetwork, _dataProvider.TrainingData);
            }

            Config.State = EvolutionState.Saving;

            Save().Wait();

            _cudaClient.Dispose();
            NeuralNetwork.Dispose();

            Config.State = EvolutionState.Idle;
        }

        private void Calculate()
        {
            for (var l = 0; l < Config.NetworkConfig.HiddenLayersNeuronCount.Length + 1; l++)
            {
                NeuralNetwork.CalcNeuronValues(_cudaClient, l);
                NeuralNetwork.CudaNeuronValuesStream.Synchronize();
            }

            for (var l = Config.NetworkConfig.HiddenLayersNeuronCount.Length; l >= 0; l--)
            {
                NeuralNetwork.CalcExpectedDifference(_cudaClient, l);
                NeuralNetwork.CudaExpectedDifferenceStream.Synchronize();

                NeuralNetwork.CalcGradientWeight(_cudaClient, l);
                NeuralNetwork.CalcGradientBias(_cudaClient, l);
                NeuralNetwork.CalcExpectedOutput(_cudaClient, l);

                NeuralNetwork.CudaGradientWeightStream.Synchronize();
                NeuralNetwork.CudaGradientBiasStream.Synchronize();
                NeuralNetwork.CudaExpectedDifferenceStream.Synchronize();
            }

            for (var l = 0; l < Config.NetworkConfig.HiddenLayersNeuronCount.Length + 1; l++)
            {
                NeuralNetwork.ApplyGradient(_cudaClient, l);
            }

            NeuralNetwork.CudaApplyGradientStream.Synchronize();
        }

        public void CancelCalculation()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}