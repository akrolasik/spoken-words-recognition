using System.Collections.Generic;
using System.Linq;
using ManagedCuda;
using MathNet.Numerics.LinearAlgebra;
using NeuralNetwork.API.Config;
using NeuralNetwork.API.Cuda;

namespace NeuralNetwork.API.Network
{
    public class NeuralNetwork
    {
        public List<MatrixFunction> Layers;
        public List<MatrixFunction> Gradient;
        public List<float> LayerGradients;

        public List<ParallelMatrices> Output;
        private List<ParallelMatrices> _temp;
        private List<ParallelMatrices> _difference;

        private CudaDeviceVariable<float> _cost;

        private CudaStream _cuda;

        public CudaStream Cuda
        {
            get
            {
                if (_cuda == null)
                {
                    _cuda = new CudaStream();
                }

                return _cuda;
            }
        }

        private readonly EvolutionConfig _evolutionConfig;
        

        public NeuralNetwork(EvolutionConfig evolutionConfig)
        {
            _evolutionConfig = evolutionConfig;

            var dimensions = new List<int> { evolutionConfig.NetworkConfig.InputCount };
            dimensions.AddRange(evolutionConfig.NetworkConfig.HiddenLayersNeuronCount);
            dimensions.Add(evolutionConfig.NetworkConfig.OutputCount);

            Layers = new List<MatrixFunction>();

            for (var i = 0; i < dimensions.Count - 1; i++)
            {
                Layers.Add(new MatrixFunction(dimensions[i + 1], dimensions[i]));
            }

            LayerGradients = Enumerable.Range(0, Layers.Count).Select(GetLayerGradient).ToList();
        }

        public NeuralNetwork(EvolutionConfig evolutionConfig, List<MatrixFunction> layers)
        {
            _evolutionConfig = evolutionConfig;
            Layers = layers;

            LayerGradients = Enumerable.Range(0, Layers.Count).Select(GetLayerGradient).ToList();
        }

        public void PrepareCuda(CudaClient cuda)
        {
            Output = Layers.Select(layer => new ParallelMatrices(1, cuda.Input.ColumnCount, 
                (y, x) => Matrix<float>.Build.Dense(layer.Weight.RowCount, 1))).ToList();

            _temp = Enumerable.Range(0, Layers.Count - 1).Select(l => new ParallelMatrices(1, Output[l].ParametersColumnCount,
                (y, x) => Matrix<float>.Build.Dense(Output[l].ParametersRowCount, 1))).ToList();

            _difference = Enumerable.Range(0, Layers.Count).Select(l => new ParallelMatrices(1, Output[l].ParametersColumnCount,
                (y, x) => Matrix<float>.Build.Dense(Output[l].ParametersRowCount, 1))).ToList();

            Gradient = Layers.Select(layer => new MatrixFunction
            {
                Weight = Matrix<float>.Build.Dense(layer.Weight.RowCount, layer.Weight.ColumnCount),
                Bias = Matrix<float>.Build.Dense(layer.Bias.RowCount, layer.Bias.ColumnCount)
            }).ToList();
        }

        public void ApplyGradient(CudaClient cuda, int layer)
        {
            var weight = new GradientParams
            {
                Base = Layers[layer].CudaWeight.DevicePointer,
                Gradient = Gradient[layer].CudaWeight.DevicePointer,
                Count = Layers[layer].WeightColumnCount * Layers[layer].WeightRowCount
            };

            cuda.ApplyGradient(Cuda, weight);

            var bias = new GradientParams
            {
                Base = Layers[layer].CudaBias.DevicePointer,
                Gradient = Gradient[layer].CudaBias.DevicePointer,
                Count = Layers[layer].BiasColumnCount * Layers[layer].BiasRowCount
            };

            cuda.ApplyGradient(Cuda, bias);
        }

        public void Synchronize()
        {
            Cuda.Synchronize();
        }

        public void CalcNeuronValues(CudaClient cuda, int layer)
        {
            var input = layer == 0 ? cuda.Input : Output[layer - 1];

            var neuronValueParams = new NeuronValueParams
            {
                Weight = Layers[layer].CudaWeight.DevicePointer,
                Bias = Layers[layer].CudaBias.DevicePointer,
                Input = input.Cuda.DevicePointer,
                Output = Output[layer].Cuda.DevicePointer,
                OutputRowCount = Layers[layer].WeightRowCount,
                OutputColumnCount = input.ParametersColumnCount,
                CommonDim = input.ParametersRowCount,
            };

            cuda.CalcNeuronValues(Cuda, neuronValueParams);

        }

        public float[] CalcCost(CudaClient cuda)
        {
            var count = cuda.ExpectedOutput.ParametersColumnCount;

            if (_cost == null)
            {
                _cost = new float[count];
            }

            cuda.CalcCost(Cuda, new CostParams
            {
                Expected = cuda.ExpectedOutput.Cuda.DevicePointer,
                Actual = Output.Last().Cuda.DevicePointer,
                Cost = _cost.DevicePointer,
                ExpectedRowCount = cuda.ExpectedOutput.ParametersRowCount,
                Count = count
            });

            return _cost;
        }

        public void CalcExpectedDifference(CudaClient cuda, int layer)
        {
            var expected = layer == Layers.Count - 1 ? cuda.ExpectedOutput : _temp[layer];

            var expectedDifferenceParams = new ExpectedDifferenceParams
            {
                Expected = expected.Cuda.DevicePointer,
                Actual = Output[layer].Cuda.DevicePointer,
                Output = _difference[layer].Cuda.DevicePointer,
                Count = expected.ParametersCount,
            };

            cuda.CalcExpectedDifference(Cuda, expectedDifferenceParams);
        }

        public void CalcGradientWeight(CudaClient cuda, int layer)
        {
            var input = layer == 0 ? cuda.Input : Output[layer - 1];
            var weightRowCount = Layers[layer].WeightRowCount;
            var weightColumnCount = Layers[layer].WeightColumnCount;
            var layerGradient = LayerGradients[layer];

            var gradientWeightParams = new GradientWeightParams
            {
                Difference = _difference[layer].Cuda.DevicePointer,
                Input = input.Cuda.DevicePointer,
                WeightGradient = Gradient[layer].CudaWeight.DevicePointer,
                WeightRowCount = weightRowCount,
                WeightColumnCount = weightColumnCount,
                InputColumnCount = input.ParametersColumnCount,
                GradientFactor = layerGradient * _evolutionConfig.GradientConfig.WeightGradientFactor * _evolutionConfig.GradientConfig.GradientFactor,
            };

            cuda.CalcGradientWeight(Cuda, gradientWeightParams);
        }

        public void CalcGradientBias(CudaClient cuda, int layer)
        {
            var input = layer == 0 ? cuda.Input : Output[layer - 1];
            var weightRowCount = Layers[layer].WeightRowCount;
            var weightColumnCount = Layers[layer].WeightColumnCount;
            var layerGradient = LayerGradients[layer];

            var gradientBiasParams = new GradientBiasParams
            {
                Difference = _difference[layer].Cuda.DevicePointer,
                BiasGradient = Gradient[layer].CudaBias.DevicePointer,
                WeightRowCount = weightRowCount,
                InputColumnCount = input.ParametersColumnCount,
                GradientFactor = layerGradient * _evolutionConfig.GradientConfig.BiasGradientFactor * _evolutionConfig.GradientConfig.GradientFactor / weightColumnCount,
            };

            cuda.CalcGradientBias(Cuda, gradientBiasParams);
        }

        public void CalcExpectedOutput(CudaClient cuda, int layer)
        {
            var input = layer == 0 ? cuda.Input : Output[layer - 1];
            var weightRowCount = Layers[layer].WeightRowCount;
            var expectedRowCount = Layers[layer - 1].WeightRowCount;
            var expectedColumnCount = input.ColumnCount;
            var layerGradient = LayerGradients[layer];

            var expectedOutputParams = new ExpectedOutputParams
            {
                Difference = _difference[layer].Cuda.DevicePointer,
                Weight = Layers[layer].CudaWeight.DevicePointer,
                Temp = _temp[layer - 1].Cuda.DevicePointer,
                WeightColumnCount = weightRowCount,
                ExpectedRowCount = expectedRowCount,
                ExpectedColumnCount = expectedColumnCount,
                GradientFactor = layerGradient * _evolutionConfig.GradientConfig.InputGradientFactor * _evolutionConfig.GradientConfig.GradientFactor
            };

            cuda.CalcExpectedOutput(Cuda, expectedOutputParams);
        }

        private float GetLayerGradient(int layer)
        {
            return layer == Layers.Count - 1 ? 1.0f : GetLayerGradient(layer + 1) / Layers[layer].Weight.RowCount;
        }
    }
}