using System;
using System.Linq;
using ManagedCuda;
using ManagedCuda.VectorTypes;
using NeuralNetwork.API.Config;
using NeuralNetwork.API.Network;

namespace NeuralNetwork.API.Cuda
{
    public class CudaClient
    {
        public ParallelMatrices Input;
        public ParallelMatrices ExpectedOutput;

        private readonly Kernel _calcNeuronValues;
        private readonly Kernel _calcExpectedDif;
        private readonly Kernel _calcGradientWeight;
        private readonly Kernel _calcGradientBias;
        private readonly Kernel _calcExpectedOutput;
        private readonly Kernel _applyGradient;
        private readonly Kernel _calcCost;

        private const int ThreadsPerBlock = 1024;

        private readonly CudaContext _ctx;

        public CudaClient(EvolutionConfig config)
        {
            _ctx = new CudaContext(true);

            _calcNeuronValues = new Kernel(_ctx, "calcNeuronValues");
            _calcExpectedDif = new Kernel(_ctx, "calcExpectedDif");
            _calcGradientWeight = new Kernel(_ctx, "calcGradientWeight");
            _calcGradientBias = new Kernel(_ctx, "calcGradientBias");
            _calcExpectedOutput = new Kernel(_ctx, "calcExpectedOutput");
            _applyGradient = new Kernel(_ctx, "applyGradient");
            _calcCost = new Kernel(_ctx, "calcCost");

            _calcNeuronValues.Cuda.BlockDimensions = new dim3(ThreadsPerBlock, 1, 1);
            _calcExpectedDif.Cuda.BlockDimensions = new dim3(ThreadsPerBlock, 1, 1);
            _calcGradientWeight.Cuda.BlockDimensions = new dim3(ThreadsPerBlock, 1, 1);
            _calcGradientBias.Cuda.BlockDimensions = new dim3(ThreadsPerBlock, 1, 1);
            _calcExpectedOutput.Cuda.BlockDimensions = new dim3(ThreadsPerBlock, 1, 1);
            _applyGradient.Cuda.BlockDimensions = new dim3(ThreadsPerBlock, 1, 1);
            _calcCost.Cuda.BlockDimensions = new dim3(ThreadsPerBlock, 1, 1);

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

            var resultCountBlocksPerGrid = (config.TrainingConfig.WordSetSize + ThreadsPerBlock - 1) / ThreadsPerBlock;

            _calcCost.Cuda.GridDimensions = new dim3(resultCountBlocksPerGrid, 1, 1);
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

        public void CalcCost(CudaStream stream, CostParams @params)
        {
            _calcCost.Cuda.RunAsync(stream.Stream,
                @params.Expected,
                @params.Actual,
                @params.Cost,
                @params.ExpectedRowCount,
                @params.Count);
        }

        public void Dispose()
        {
            _ctx.Dispose();
        }
    }
}