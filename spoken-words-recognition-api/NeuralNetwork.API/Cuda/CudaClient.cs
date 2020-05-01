using System;
using System.Collections.Generic;
using System.Linq;
using ManagedCuda;
using ManagedCuda.VectorTypes;
using MathNet.Numerics.LinearAlgebra;
using NeuralNetwork.API.Config;
using NeuralNetwork.API.Data;
using NeuralNetwork.API.Network;

namespace NeuralNetwork.API.Cuda
{
    public class CudaClient
    {
        public List<TrainingData> NewTrainingData;
        public List<Matrix<float>> NewInput;
        public List<Matrix<float>> NewExpectedOutput;

        public List<TrainingData> TrainingData;
        public ParallelMatrices Input;
        public ParallelMatrices ExpectedOutput;


        private readonly Kernel _calcNeuronValues;
        private readonly Kernel _calcExpectedDif;
        private readonly Kernel _calcGradientWeight;
        private readonly Kernel _calcGradientBias;
        private readonly Kernel _calcExpectedOutput;
        private readonly Kernel _applyGradient;
        private readonly Kernel _replaceData;

        private const int ThreadsPerBlock = 1024;

        private readonly CudaContext _ctx;
        private readonly Random _random = new Random();
        private readonly CudaStream _replaceDataStream;


        public CudaClient(EvolutionConfig config)
        {
            _ctx = new CudaContext(true);
            _replaceDataStream = new CudaStream();

            _calcNeuronValues = new Kernel(_ctx, "calcNeuronValues");
            _calcExpectedDif = new Kernel(_ctx, "calcExpectedDif");
            _calcGradientWeight = new Kernel(_ctx, "calcGradientWeight");
            _calcGradientBias = new Kernel(_ctx, "calcGradientBias");
            _calcExpectedOutput = new Kernel(_ctx, "calcExpectedOutput");
            _applyGradient = new Kernel(_ctx, "applyGradient");
            _replaceData = new Kernel(_ctx, "replaceData");

            _calcNeuronValues.Cuda.BlockDimensions = new dim3(ThreadsPerBlock, 1, 1);
            _calcExpectedDif.Cuda.BlockDimensions = new dim3(ThreadsPerBlock, 1, 1);
            _calcGradientWeight.Cuda.BlockDimensions = new dim3(ThreadsPerBlock, 1, 1);
            _calcGradientBias.Cuda.BlockDimensions = new dim3(ThreadsPerBlock, 1, 1);
            _calcExpectedOutput.Cuda.BlockDimensions = new dim3(ThreadsPerBlock, 1, 1);
            _applyGradient.Cuda.BlockDimensions = new dim3(ThreadsPerBlock, 1, 1);
            _replaceData.Cuda.BlockDimensions = new dim3(ThreadsPerBlock, 1, 1);

            var resultParamCount = config.NetworkConfig.HiddenLayersNeuronCount.First() * config.TrainingConfig.WordSetSize;
            var resultParamCountBlocksPerGrid = (resultParamCount + ThreadsPerBlock - 1) / ThreadsPerBlock;

            _calcNeuronValues.Cuda.GridDimensions = new dim3(resultParamCountBlocksPerGrid, 1, 1);
            _calcExpectedDif.Cuda.GridDimensions = new dim3(resultParamCountBlocksPerGrid, 1, 1);
            _calcGradientBias.Cuda.GridDimensions = new dim3(resultParamCountBlocksPerGrid, 1, 1);
            _calcExpectedOutput.Cuda.GridDimensions = new dim3(resultParamCountBlocksPerGrid, 1, 1);

            var networkParamCount = config.NetworkConfig.HiddenLayersNeuronCount.First() * config.NetworkConfig.InputCount;
            var networkParamCountBlocksPerGrid = (networkParamCount + ThreadsPerBlock - 1) / ThreadsPerBlock;

            _calcGradientWeight.Cuda.GridDimensions = new dim3(networkParamCountBlocksPerGrid, 1, 1);
            _applyGradient.Cuda.GridDimensions = new dim3(networkParamCountBlocksPerGrid, 1, 1);

            var inputParamCountBlocksPerGrid = (config.NetworkConfig.InputCount + ThreadsPerBlock - 1) / ThreadsPerBlock;
            _replaceData.Cuda.GridDimensions = new dim3(inputParamCountBlocksPerGrid, 1, 1);
        }

        public void UpdateData(bool all = false)
        {
            if (all)
            {
                Input = new ParallelMatrices(1, NewInput.Count, NewInput);
                ExpectedOutput = new ParallelMatrices(1, NewExpectedOutput.Count, NewExpectedOutput);
                TrainingData = NewTrainingData;
            }
            else
            {
                var data = Enumerable.Range(0, Input.ColumnCount).Select(x => x).ToList();
                var toReplace = new List<int>();

                while (toReplace.Count < NewInput.Count)
                {
                    var index = _random.Next(data.Count);
                    toReplace.Add(data[index]);
                    data.RemoveAt(index);
                }

                var fromInput = NewInput.Select(x => (CudaDeviceVariable<float>)x.ToColumnMajorArray()).ToList();
                var fromNewExpectedOutput = NewExpectedOutput.Select(x => (CudaDeviceVariable<float>)x.ToColumnMajorArray()).ToList();

                for (var i = 0; i < NewInput.Count; i++)
                {
                    TrainingData[toReplace[i]] = NewTrainingData[i];

                    ReplaceData(new ReplaceData
                    {
                        From = fromInput[i].DevicePointer,
                        To = Input.Cuda.DevicePointer,
                        Offset = toReplace[i] * Input.ParametersRowCount,
                        Count = Input.ParametersRowCount
                    });

                    ReplaceData(new ReplaceData
                    {
                        From = fromNewExpectedOutput[i].DevicePointer,
                        To = ExpectedOutput.Cuda.DevicePointer,
                        Offset = toReplace[i] * Input.ParametersRowCount,
                        Count = Input.ParametersRowCount
                    });
                }

                _replaceDataStream.Synchronize();

                fromInput.ForEach(x => x.Dispose());
                fromNewExpectedOutput.ForEach(x => x.Dispose());
            }

            NewInput = null;
            NewExpectedOutput = null;
            NewTrainingData = null;
        }

        public void ReplaceData(ReplaceData @params)
        {
            _replaceData.Cuda.RunAsync(_replaceDataStream.Stream, @params.From, @params.To, @params.Offset, @params.Count);
        }

        public void CalcNeuronValues(CudaStream stream, NeuronValueParams @params)
        {
            _calcNeuronValues.Cuda.RunAsync(stream.Stream,
                @params.Weight,
                @params.Bias,
                @params.Input,
                @params.Output,
                @params.OutputRowCount,
                @params.OutputColumnCount,
                @params.CommonDim);
        }

        public void CalcExpectedDifference(CudaStream stream, ExpectedDifferenceParams @params)
        {
            _calcExpectedDif.Cuda.RunAsync(stream.Stream,
                @params.Expected,
                @params.Actual,
                @params.Output,
                @params.Count);
        }

        public void CalcGradientWeight(CudaStream stream, GradientWeightParams @params)
        {
            _calcGradientWeight.Cuda.RunAsync(stream.Stream,
                @params.Difference,
                @params.Input,
                @params.WeightGradient,
                @params.WeightRowCount,
                @params.WeightColumnCount,
                @params.InputColumnCount,
                @params.GradientFactor);
        }

        public void CalcGradientBias(CudaStream stream, GradientBiasParams @params)
        {
            _calcGradientBias.Cuda.RunAsync(stream.Stream,
                @params.Difference,
                @params.BiasGradient,
                @params.WeightRowCount,
                @params.InputColumnCount,
                @params.GradientFactor);
        }

        public void CalcExpectedOutput(CudaStream stream, ExpectedOutputParams @params)
        {
            _calcExpectedOutput.Cuda.RunAsync(stream.Stream,
                @params.Difference,
                @params.Weight,
                @params.Temp,
                @params.WeightColumnCount,
                @params.ExpectedRowCount,
                @params.ExpectedColumnCount,
                @params.GradientFactor);
        }

        public void ApplyGradient(CudaStream stream, GradientParams @params)
        {
            _applyGradient.Cuda.RunAsync(stream.Stream,
                @params.Base,
                @params.Gradient,
                @params.Count);
        }

        public void Dispose()
        {
            Input.Dispose();
            ExpectedOutput.Dispose();

            _ctx.Dispose();
        }

        public void UpdateData(List<TrainingData> randomDataSet)
        {
            NewTrainingData = randomDataSet;
            NewInput = randomDataSet.Select(x => x.Input()).ToList();
            NewExpectedOutput = randomDataSet.Select(x => x.ExpectedOutput).ToList();
        }
    }
}