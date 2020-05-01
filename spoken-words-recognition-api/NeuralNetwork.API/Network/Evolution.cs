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

        private readonly DataProvider _dataProvider;
        private CancellationTokenSource _cancellationTokenSource;
        
        private bool _isSavingProcessWorking;
        private bool _firstRunAfterDataShuffle;

        private CudaClient _cudaClient;
        private bool _newSetIsReady;

        private string BasePath => $"temp/evolutions/{Config.Id}";
        private string TempPath => $"temp/evolutions/{Config.Id}_temp";
        private string BackupPath => $"temp/evolutions/{Config.Id}_backup";

        public Evolution(EvolutionConfig evolutionConfig)
        {
            Config = evolutionConfig;

            if (Directory.Exists(BasePath))
            {
                try
                {
                    NeuralNetwork = LoadNeuralNetwork(BasePath);
                    EvolutionStatistics = LoadStatistics(BasePath);
                }
                catch (Exception)
                {
                    if (Directory.Exists(BackupPath))
                    {
                        NeuralNetwork = LoadNeuralNetwork(BackupPath);
                        EvolutionStatistics = LoadStatistics(BackupPath);
                    }
                    else
                    {
                        throw new Exception("Corrupted data");
                    }
                }
            }
            else
            {
                NeuralNetwork = new NeuralNetwork(Config);
                EvolutionStatistics = new EvolutionStatistics();
            }

            _dataProvider = new DataProvider(evolutionConfig);
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

            return new NeuralNetwork(Config, layers);
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
            if (_isSavingProcessWorking) return;

            _isSavingProcessWorking = true;


            if (Directory.Exists(BackupPath))
                Directory.Delete(BackupPath, true);

            if (Directory.Exists(TempPath))
                Directory.Delete(TempPath, true);

            Directory.CreateDirectory(TempPath);

            SaveNeuralNetwork();
            await SaveStatistics();

            if (Directory.Exists(BasePath))
                Directory.Move(BasePath, BackupPath);

            Directory.Move(TempPath, BasePath);

            if (Directory.Exists(BackupPath))
                Directory.Delete(BackupPath, true);

            _isSavingProcessWorking = false;
        }

        private async Task SaveStatistics()
        {
            var json = JsonConvert.SerializeObject(EvolutionStatistics);
            await File.WriteAllTextAsync($"{TempPath}/statistics.json", json);
        }

        private void SaveNeuralNetwork()
        {
            if (!Directory.Exists(TempPath))
            {
                Directory.CreateDirectory(TempPath);
            }

            for (var l = 0; l < NeuralNetwork.Layers.Count; l++)
            {
                var weightArray = NeuralNetwork.Layers[l].Weight.ToColumnMajorArray();
                var biasArray = NeuralNetwork.Layers[l].Bias.ToColumnMajorArray();
                var byteArray = weightArray.SelectMany(BitConverter.GetBytes).ToArray().ToList();
                byteArray.AddRange(biasArray.SelectMany(BitConverter.GetBytes).ToArray());
                using var fileStream = new FileStream($"{TempPath}/layer{l}.bin", FileMode.Create);
                using var stream = new BinaryWriter(fileStream);
                stream.Write(byteArray.ToArray());
            }
        }

        public void StartCalculation()
        {
            Config.IsRunning = true;

            _cancellationTokenSource = new CancellationTokenSource();
            _cudaClient = new CudaClient(Config);

            NeuralNetwork.PrepareCuda(_cudaClient);
            ShuffleData(Config.TrainingConfig.WordSetSize);

            _cudaClient.UpdateData(true);
            _firstRunAfterDataShuffle = true;
            _newSetIsReady = false;

            PrepareShuffleData();

            EvolutionStatistics.StartMeasuringTime();

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                if (Config.TrainingConfig.MaxCalculationTimeInMinutes <= EvolutionStatistics.TotalComputingTimeInSeconds)
                {
                    Config.TrainingConfig.MaxCalculationTimeInMinutes = null;
                    _cancellationTokenSource.Cancel();
                    break;
                }

                if (_newSetIsReady)
                {
                    // _cudaClient.UpdateData();
                    _firstRunAfterDataShuffle = true;
                    _newSetIsReady = false;

                    PrepareShuffleData();
                }

                Calculate();

                EvolutionStatistics.Update(NeuralNetwork, _cudaClient.TrainingData, _firstRunAfterDataShuffle);

                _firstRunAfterDataShuffle = false;
            }

            EvolutionStatistics.Update(NeuralNetwork, _cudaClient.TrainingData, true);
            Save().Wait();

            _cudaClient.Dispose();
            NeuralNetwork.Dispose();

            Config.IsRunning = false;
        }

        private void PrepareShuffleData()
        {
            _ = Task.Run(() => ShuffleData((int)(Config.TrainingConfig.WordSetSize * 0.1)));
        }

        private void ShuffleData(int count)
        {
            _cudaClient.UpdateData(_dataProvider.GetRandomDataSet(Math.Min(count, _dataProvider.TrainingData.Count), _cudaClient.TrainingData));

            _newSetIsReady = true;
        }

        private void Calculate()
        {
            for (var l = 0; l < Config.NetworkConfig.HiddenLayersNeuronCount.Length + 1; l++)
            {
                NeuralNetwork.CalcNeuronValues(_cudaClient, l);

                if (_firstRunAfterDataShuffle)
                    NeuralNetwork.CudaNeuronValuesStream.Synchronize();
            }

            for (var l = Config.NetworkConfig.HiddenLayersNeuronCount.Length; l >= 0; l--)
            {
                NeuralNetwork.CalcExpectedDifference(_cudaClient, l);

                if (_firstRunAfterDataShuffle)
                    NeuralNetwork.CudaExpectedDifferenceStream.Synchronize();

                NeuralNetwork.CalcGradientWeight(_cudaClient, l);
                NeuralNetwork.CalcGradientBias(_cudaClient, l);

                if (l > 0)
                    NeuralNetwork.CalcExpectedOutput(_cudaClient, l);

                if (_firstRunAfterDataShuffle)
                {
                    NeuralNetwork.CudaGradientWeightStream.Synchronize();
                    NeuralNetwork.CudaGradientBiasStream.Synchronize();
                    NeuralNetwork.CudaExpectedDifferenceStream.Synchronize();
                }
            }

            for (var l = 0; l < Config.NetworkConfig.HiddenLayersNeuronCount.Length + 1; l++)
                NeuralNetwork.ApplyGradient(_cudaClient, l);

            if (_firstRunAfterDataShuffle)
                NeuralNetwork.CudaApplyGradientStream.Synchronize();
        }

        public void CancelCalculation()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}