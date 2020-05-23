using System.Collections.Generic;
using System.Linq;
using ManagedCuda;
using MathNet.Numerics.LinearAlgebra;
using NeuralNetwork.API.Config;
using NeuralNetwork.API.Cuda;
using NeuralNetwork.API.Data;

namespace NeuralNetwork.API.Network
{
    public class NeuralNetwork
    {
        public List<MatrixFunction> Layers;
        public List<MatrixFunction> Gradient;
        public List<ParallelMatrices> Output;

        private List<ParallelMatrices> _temp;
        private List<ParallelMatrices> _difference;

        private readonly List<float> _layerGradients;

        private CudaStream _calcNeuronValuesStream;
        private CudaStream _calcExpectedDifferenceStream;
        private CudaStream _calcGradientWeightStream;
        private CudaStream _calcGradientBiasStream;
        private CudaStream _calcExpectedOutputStream;
        private CudaStream _applyGradientStream;

        public CudaStream CudaNeuronValuesStream => _calcNeuronValuesStream ??= new CudaStream();
        public CudaStream CudaExpectedDifferenceStream => _calcExpectedDifferenceStream ??= new CudaStream();
        public CudaStream CudaGradientWeightStream => _calcGradientWeightStream ??= new CudaStream();
        public CudaStream CudaGradientBiasStream => _calcGradientBiasStream ??= new CudaStream();
        public CudaStream CudaExpectedOutputStream => _calcExpectedOutputStream ??= new CudaStream();
        public CudaStream CudaApplyGradientStream => _applyGradientStream ??= new CudaStream();

        public NeuralNetwork(EvolutionConfig evolutionConfig)
        {
            var dimensions = new List<int> { evolutionConfig.NetworkConfig.InputCount };
            dimensions.AddRange(evolutionConfig.NetworkConfig.HiddenLayersNeuronCount);
            dimensions.Add(evolutionConfig.NetworkConfig.OutputCount);

            Layers = new List<MatrixFunction>();

            for (var i = 0; i < dimensions.Count - 1; i++)
            {
                Layers.Add(new MatrixFunction(dimensions[i + 1], dimensions[i]));
            }

            _layerGradients = Enumerable.Range(0, Layers.Count).Select(GetLayerGradient).ToList();
        }

        public NeuralNetwork(List<MatrixFunction> layers)
        {
            Layers = layers;

            _layerGradients = Enumerable.Range(0, Layers.Count).Select(GetLayerGradient).ToList();
        }

        public void Dispose()
        {
            _calcNeuronValuesStream.Dispose();
            _calcExpectedDifferenceStream?.Dispose();
            _calcGradientWeightStream?.Dispose();
            _calcGradientBiasStream?.Dispose();
            _calcExpectedOutputStream?.Dispose();
            _applyGradientStream?.Dispose();

            Layers.ForEach(x => x.Dispose());
            Output.ForEach(x => x.Dispose());
            Gradient?.ForEach(x => x.Dispose());
            _temp?.ForEach(x => x.Dispose());
            _difference?.ForEach(x => x.Dispose());
        }

        public void PrepareCuda(DataProvider dataProvider, bool verification = false)
        {
            Output = Layers.Select(layer => new ParallelMatrices(1, dataProvider.TrainingData.Count, 
                (y, x) => Matrix<float>.Build.Dense(layer.Weight.RowCount, 1))).ToList();

            if (verification) return;

            Gradient = Layers.Select(layer => new MatrixFunction
            {
                Weight = Matrix<float>.Build.Dense(layer.Weight.RowCount, layer.Weight.ColumnCount),
                Bias = Matrix<float>.Build.Dense(layer.Bias.RowCount, layer.Bias.ColumnCount)
            }).ToList();

            _temp = Enumerable.Range(0, Layers.Count - 1).Select(l => new ParallelMatrices(1, Output[l].ParametersColumnCount,
                (y, x) => Matrix<float>.Build.Dense(Output[l].ParametersRowCount, 1))).ToList();

            _difference = Enumerable.Range(0, Layers.Count).Select(l => new ParallelMatrices(1, Output[l].ParametersColumnCount,
                (y, x) => Matrix<float>.Build.Dense(Output[l].ParametersRowCount, 1))).ToList();
        }

        public void ApplyGradient(CudaClient _cudaClient, int layer)
        {
            var weightGradientParams = new GradientParams
            {
                Base = Layers[layer].CudaWeight.DevicePointer,
                Gradient = Gradient[layer].CudaWeight.DevicePointer,
                Count = Layers[layer].WeightColumnCount * Layers[layer].WeightRowCount
            };

            var biasGradientParams = new GradientParams
            {
                Base = Layers[layer].CudaBias.DevicePointer,
                Gradient = Gradient[layer].CudaBias.DevicePointer,
                Count = Layers[layer].BiasColumnCount * Layers[layer].BiasRowCount
            };

            _cudaClient.ApplyGradient(CudaApplyGradientStream, weightGradientParams);
            _cudaClient.ApplyGradient(CudaApplyGradientStream, biasGradientParams);
        }

        public void CalcNeuronValues(CudaClient _cudaClient, int layer)
        {
            var input = layer == 0 ? _cudaClient.Input : Output[layer - 1];

            // Layers[layer].CudaWeight.CopyToDevice();

            var neuronValueParam = new NeuronValueParams
            {
                Weight = Layers[layer].CudaWeight.DevicePointer,
                Bias = Layers[layer].CudaBias.DevicePointer,
                Input = input.Cuda.DevicePointer,
                Output = Output[layer].Cuda.DevicePointer,
                OutputRowCount = Layers[layer].WeightRowCount,
                OutputColumnCount = input.ParametersColumnCount,
                CommonDim = input.ParametersRowCount,
            };

            _cudaClient.CalcNeuronValues(CudaNeuronValuesStream, neuronValueParam);
        }

        public void CalcExpectedDifference(CudaClient _cudaClient, int layer)
        {
            var expected = layer == Layers.Count - 1 ? _cudaClient.ExpectedOutput : _temp[layer];

            var expectedDifferenceParams = new ExpectedDifferenceParams
            {
                Expected = expected.Cuda.DevicePointer,
                Actual = Output[layer].Cuda.DevicePointer,
                Output = _difference[layer].Cuda.DevicePointer,
                Count = expected.ParametersCount,
            };

            _cudaClient.CalcExpectedDifference(CudaExpectedDifferenceStream, expectedDifferenceParams);
        }

        public void CalcGradientWeight(CudaClient _cudaClient, int layer)
        {
            var input = layer == 0 ? _cudaClient.Input : Output[layer - 1];
            var weightRowCount = Layers[layer].WeightRowCount;
            var weightColumnCount = Layers[layer].WeightColumnCount;
            var layerGradient = _layerGradients[layer];

            var gradientWeightParams = new GradientWeightParams
            {
                Difference = _difference[layer].Cuda.DevicePointer,
                Input = input.Cuda.DevicePointer,
                WeightGradient = Gradient[layer].CudaWeight.DevicePointer,
                WeightRowCount = weightRowCount,
                WeightColumnCount = weightColumnCount,
                InputColumnCount = input.ParametersColumnCount,
                GradientFactor = layerGradient / input.ParametersColumnCount,
            };

            _cudaClient.CalcGradientWeight(CudaGradientWeightStream, gradientWeightParams);
        }

        public void CalcGradientBias(CudaClient _cudaClient, int layer)
        {
            var input = layer == 0 ? _cudaClient.Input : Output[layer - 1];
            var weightRowCount = Layers[layer].WeightRowCount;
            var weightColumnCount = Layers[layer].WeightColumnCount;
            var layerGradient = _layerGradients[layer];

            var gradientBiasParams = new GradientBiasParams
            {
                Difference = _difference[layer].Cuda.DevicePointer,
                BiasGradient = Gradient[layer].CudaBias.DevicePointer,
                WeightRowCount = weightRowCount,
                InputColumnCount = input.ParametersColumnCount,
                GradientFactor = layerGradient / input.ParametersColumnCount / weightColumnCount,
            };

            _cudaClient.CalcGradientBias(CudaGradientBiasStream, gradientBiasParams);
        }

        public void CalcExpectedOutput(CudaClient _cudaClient, int layer)
        {
            if (layer == 0) return;
                
            var input = layer == 0 ? _cudaClient.Input : Output[layer - 1];
            var weightRowCount = Layers[layer].WeightRowCount;
            var expectedRowCount = Layers[layer - 1].WeightRowCount;
            var expectedColumnCount = input.ColumnCount;
            var layerGradient = _layerGradients[layer];

            var expectedOutputParams = new ExpectedOutputParams
            {
                Difference = _difference[layer].Cuda.DevicePointer,
                Weight = Layers[layer].CudaWeight.DevicePointer,
                Temp = _temp[layer - 1].Cuda.DevicePointer,
                WeightColumnCount = weightRowCount,
                ExpectedRowCount = expectedRowCount,
                ExpectedColumnCount = expectedColumnCount,
                GradientFactor = layerGradient
            };

            _cudaClient.CalcExpectedOutput(CudaExpectedOutputStream, expectedOutputParams);
        }

        private float GetLayerGradient(int layer)
        {
            return layer == Layers.Count - 1 ? 1.0f : GetLayerGradient(layer + 1) / Layers[layer].Weight.RowCount;
        }
    }
}