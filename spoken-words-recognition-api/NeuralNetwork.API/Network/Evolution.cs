using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public readonly List<NeuralNetwork> Units;
        public readonly List<NeuralNetworkStatistics> Statistics;

        private readonly DataProvider _dataProvider;

        private CancellationTokenSource _cancellationTokenSource;
        
        private CudaClient _cudaClient;
        private bool _saving;

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
                    Units = LoadUnits(BasePath);
                    Statistics = LoadStatistics(BasePath);
                }
                catch (Exception)
                {
                    if (Directory.Exists(BackupPath))
                    {
                        Units = LoadUnits(BackupPath);
                        Statistics = LoadStatistics(BackupPath);
                    }
                    else
                    {
                        throw new Exception("Corrupted data");
                    }
                }
            }
            else
            {
                Units = Enumerable.Range(0, Config.TrainingConfig.PopulationSize).Select(x => new NeuralNetwork(Config)).ToList();
                Statistics = Enumerable.Range(0, Config.TrainingConfig.PopulationSize).Select(x => new NeuralNetworkStatistics()).ToList();
            }

            _dataProvider = new DataProvider(evolutionConfig);
        }

        private List<NeuralNetwork> LoadUnits(string path)
        {
            var units = new List<NeuralNetwork>();

            var unitIndices = Enumerable.Range(0, Config.TrainingConfig.PopulationSize).ToList();

            var tasks = Enumerable.Range(0, Config.TrainingConfig.SavingThreadCount).Select(x => Task.Run(() =>
            {
                while (true)
                {
                    int unitIndex;

                    lock (unitIndices)
                    {
                        if (unitIndices.Count > 0)
                        {
                            unitIndex = unitIndices[0];
                            unitIndices.RemoveAt(0);
                        }
                        else return;
                    }

                    var layers = new List<MatrixFunction>();

                    for (var l = 0; l < Config.NetworkConfig.HiddenLayersNeuronCount.Length + 1; l++)
                    {
                        var weightWidth = Config.NetworkConfig.GetLayerWeightWidth(l);
                        var weightHeight = Config.NetworkConfig.GetLayerWeightHeight(l);
                        var weightParamCount = weightWidth * weightHeight;

                        using var fileStream = new FileStream($"{path}/unit{unitIndex}/layer{l}.bin", FileMode.Open);
                        using var stream = new BinaryReader(fileStream);
                        var weightBytes = stream.ReadBytes(weightParamCount * 4);
                        var biasBytes = stream.ReadBytes(weightHeight * 4);

                        var weight = Enumerable.Range(0, weightParamCount).Select(x => BitConverter.ToSingle(weightBytes, x * 4)).ToArray();
                        var bias = Enumerable.Range(0, weightHeight).Select(x => BitConverter.ToSingle(biasBytes, x * 4)).ToArray();

                        var weightMatrix = Matrix<float>.Build.Dense(weightHeight, weightWidth, weight);
                        var biasMatrix = Matrix<float>.Build.Dense(weightHeight, 1, bias);

                        layers.Add(new MatrixFunction(weightMatrix, biasMatrix));
                    }

                    units.Add(new NeuralNetwork(Config, layers));
                }
            })).ToArray();

            Task.WaitAll(tasks);

            return units;
        }

        private List<NeuralNetworkStatistics> LoadStatistics(string path)
        {
            var json = File.ReadAllText($"{path}/statistics.json");
            return JsonConvert.DeserializeObject<List<NeuralNetworkStatistics>>(json);
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
            if (_saving) return;

            _saving = true;


            if (Directory.Exists(BackupPath))
                Directory.Delete(BackupPath, true);

            if (Directory.Exists(TempPath))
                Directory.Delete(TempPath, true);

            Directory.CreateDirectory(TempPath);

            await SaveUnits();
            await SaveStatistics();

            if (Directory.Exists(BasePath))
                Directory.Move(BasePath, BackupPath);

            Directory.Move(TempPath, BasePath);

            if (Directory.Exists(BackupPath))
                Directory.Delete(BackupPath, true);

            _saving = false;
        }

        private async Task SaveStatistics()
        {
            var json = JsonConvert.SerializeObject(Statistics);
            await File.WriteAllTextAsync($"{TempPath}/statistics.json", json);
        }

        private async Task SaveUnits()
        {
            var unitIndices = Enumerable.Range(0, Config.TrainingConfig.PopulationSize).ToList();

            var tasks = Enumerable.Range(0, Config.TrainingConfig.SavingThreadCount).Select(x => Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        int unitIndex;

                        lock (unitIndices)
                        {
                            if (unitIndices.Count > 0)
                            {
                                unitIndex = unitIndices[0];
                                unitIndices.RemoveAt(0);
                            }
                            else return;
                        }

                        var unitPath = $"{TempPath}/unit{unitIndex}";
                        if (!Directory.Exists(unitPath))
                        {
                            Directory.CreateDirectory(unitPath);
                        }

                        for (var l = 0; l < Units[unitIndex].Layers.Count; l++)
                        {
                            var weightArray = Units[unitIndex].Layers[l].Weight.ToColumnMajorArray();
                            var biasArray = Units[unitIndex].Layers[l].Bias.ToColumnMajorArray();
                            var byteArray = weightArray.SelectMany(BitConverter.GetBytes).ToArray().ToList();
                            byteArray.AddRange(biasArray.SelectMany(BitConverter.GetBytes).ToArray());
                            using var fileStream = new FileStream($"{unitPath}/layer{l}.bin", FileMode.Create);
                            using var stream = new BinaryWriter(fileStream);
                            stream.Write(byteArray.ToArray());
                        }
                    }
                }
                catch (Exception)
                {
                    // don't worry, there is backup

                    if (!Directory.Exists(TempPath))
                    {
                        Directory.Delete(TempPath, true);
                    }
                }
            }));

            await Task.WhenAll(tasks);
        }

        public void StartCalculation()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            Config.IsRunning = true;

            _cudaClient = new CudaClient(Config);

            ShuffleData();

            //var maintenance = Task.Run(async () =>
            //{
            //    while (!_cancellationTokenSource.IsCancellationRequested)
            //    {
            //        Thread.Sleep(TimeSpan.FromSeconds(1));
            //        //await Save();

            //        //if (!_cancellationTokenSource.IsCancellationRequested)
            //        //{
            //        //    ShuffleData();
            //        //}
            //    }
            //});

            foreach (var unit in Units)
            {
                unit.PrepareCuda(_cudaClient);
                unit.PrepareCalcNeuronValues(_cudaClient);
                unit.PrepareCalcExpectedDifference(_cudaClient);
                unit.PrepareCalcGradientWeight(_cudaClient);
                unit.PrepareCalcGradientBias(_cudaClient);
                unit.PrepareCalcExpectedOutput(_cudaClient);
                unit.PrepareApplyGradient(_cudaClient);
                unit.PrepareCalcCost(_cudaClient);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                //if (Config.TrainingConfig.IterationCount == Statistics.CurrentIteration)
                //{
                //    Config.TrainingConfig.IterationCount = null;
                //    _cancellationTokenSource.Cancel();
                //    break;
                //}

                Calculate();

                UpdateStatistics(stopwatch.Elapsed);
            }

            //await maintenance.ContinueWith(async _ =>
            //{
            //    lock (_cudaClient)
            //    {
            //        _cudaClient.Dispose();
            //    }

            //    await Save();
            //    Config.IsRunning = false;
            //});
        }

        private void ShuffleData()
        {
            var randomSet = _dataProvider.GetRandomDataSet();
            var inputMatrices = randomSet.Select(x => x.Input()).ToList();
            var outputMatrices = randomSet.Select(x => x.ExpectedOutput).ToList();

            _cudaClient.Input = new ParallelMatrices(1, inputMatrices.Count, inputMatrices);
            _cudaClient.ExpectedOutput = new ParallelMatrices(1, inputMatrices.Count, outputMatrices);
        }

        private void Synchronize()
        {
            for (var i = 0; i < Units.Count; i++)
            {
                Units[i].Synchronize();
            }
        }

        private void Calculate()
        {
            for (var l = 0; l < Config.NetworkConfig.HiddenLayersNeuronCount.Length + 1; l++)
            {
                for(var i = 0; i < Units.Count; i++)
                {
                    Units[i].CalcNeuronValues(_cudaClient, l);
                }

                // Synchronize();
            }

            for (var l = Config.NetworkConfig.HiddenLayersNeuronCount.Length; l >= 0; l--)
            {
                for (var i = 0; i < Units.Count; i++)
                {
                    Units[i].CalcExpectedDifference(_cudaClient, l);
                }

                // Synchronize();

                for (var i = 0; i < Units.Count; i++)
                {
                    Units[i].CalcGradientWeight(_cudaClient, l);
                    Units[i].CalcGradientBias(_cudaClient, l);

                    if (l > 0)
                    {
                        Units[i].CalcExpectedOutput(_cudaClient, l);
                    }
                }

                // Synchronize();
            }

            for (var l = 0; l < Config.NetworkConfig.HiddenLayersNeuronCount.Length + 1; l++)
            {
                for (var i = 0; i < Units.Count; i++)
                {
                    Units[i].ApplyGradient(_cudaClient, l);
                }
            }

            // Synchronize();
        }

        public void CancelCalculation()
        {
            _cancellationTokenSource.Cancel();
        }

        private void UpdateStatistics(TimeSpan calculationTime)
        {
            for(var i = 0; i < Units.Count; i++)
            {
                Statistics[i].CurrentIteration++;
                Statistics[i].TotalComputingTimeInSeconds = (float)calculationTime.TotalSeconds;

                if (Math.Abs(Math.Pow(Statistics[i].CurrentIteration + 1, 1.0 / (Statistics[i].Cost.Count + 1)) - 2.0) < 1.0E-5f)
                {
                    var cost = Units[i].CalcCost(_cudaClient);
                    Statistics[i].Cost.Add(cost.Average());

                    //foreach (var parallelMatricese in Units[i].Output)
                    //{
                    //    parallelMatricese.UpdateElements();
                    //}
                }
            }
        }
    }
}