using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public List<float> LayerGradients;

        public List<ParallelMatrices> Output;
        private List<ParallelMatrices> _temp;
        private List<ParallelMatrices> _difference;

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
            //var actualWeight = Layers[layer].Weight + Gradient[layer].Weight;
            //var actualBias = Layers[layer].Bias + Gradient[layer].Bias;

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

            //if (!MatrixExtension.Compare(actualWeight, Layers[layer].Weight))
            //{
            //    Debugger.Break();
            //}

            //if (!MatrixExtension.Compare(actualBias, Layers[layer].Bias))
            //{
            //    Debugger.Break();
            //}
        }

        public void Synchronize()
        {
            Cuda.Synchronize();
        }

        public void CalcNeuronValues(CudaClient cuda, int layer)
        {
            var input = layer == 0 ? cuda.Input : Output[layer - 1];

            //var actual = new ParallelMatrices(1, input.Elements.Count, input.Elements.Select(x => Layers[layer].Calculate(x)).ToList());

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

            //if (!ParallelMatrices.Compare(actual, Output[layer]))
            //{
            //    throw new Exception("Not equal!");
            //}
        }

        public float[] CalcCost(CudaClient cuda)
        {
            var count = cuda.ExpectedOutput.ParametersColumnCount;
            var cost = (CudaDeviceVariable<float>) new float[count];

            cuda.CalcCost(Cuda, new CostParams
            {
                Expected = cuda.ExpectedOutput.Cuda.DevicePointer,
                Actual = Output.Last().Cuda.DevicePointer,
                Cost = cost.DevicePointer,
                ExpectedRowCount = cuda.ExpectedOutput.ParametersRowCount,
                Count = count
            });

            return cost;
        }

        public void CalcExpectedDifference(CudaClient cuda, int layer)
        {
            var expected = layer == Layers.Count - 1 ? cuda.ExpectedOutput : _temp[layer];

            //Output[layer].UpdateElements();

            //var actual = new ParallelMatrices(1, Output[layer].ParametersColumnCount, 
            //    (y, x) => expected.GetElement(y, x) - Output[layer].GetElement(y, x));

            var expectedDifferenceParams = new ExpectedDifferenceParams
            {
                Expected = expected.Cuda.DevicePointer,
                Actual = Output[layer].Cuda.DevicePointer,
                Output = _difference[layer].Cuda.DevicePointer,
                Count = expected.ParametersCount,
            };

            cuda.CalcExpectedDifference(Cuda, expectedDifferenceParams);

            //if (!ParallelMatrices.Compare(actual, _difference[layer]))
            //{
            //    throw new Exception("Not equal!");
            //}
        }

        public void CalcGradientWeight(CudaClient cuda, int layer)
        {
            var input = layer == 0 ? cuda.Input : Output[layer - 1];
            var weightRowCount = Layers[layer].WeightRowCount;
            var weightColumnCount = Layers[layer].WeightColumnCount;
            var layerGradient = LayerGradients[layer];

            //_difference[layer].UpdateElements();

            //var actual = Matrix<float>.Build.Dense(weightRowCount, weightColumnCount, (y1, x1) =>
            //{
            //    var values = new List<float>();

            //    for (var x = 0; x < _difference[layer].ColumnCount; x++)
            //    {
            //        var outputValue = _difference[layer].GetElement(0, x)[y1, 0];
            //        var inputValue = input.GetElement(0, x)[x1, 0];
            //        values.Add(outputValue * inputValue);
            //    }
                
            //    return values.Average() * _evolutionConfig.GradientConfig.WeightGradientFactor * layerGradient;
            //});

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

            //if (!MatrixExtension.Compare(actual, Gradient[layer].Weight))
            //{
            //    throw new Exception("Not equal!");
            //}
        }

        public void CalcGradientBias(CudaClient cuda, int layer)
        {
            var input = layer == 0 ? cuda.Input : Output[layer - 1];
            var weightRowCount = Layers[layer].WeightRowCount;
            var weightColumnCount = Layers[layer].WeightColumnCount;
            var layerGradient = LayerGradients[layer];

            //_difference[layer].UpdateElements();

            //var actual = Matrix<float>.Build.Dense(weightRowCount, 1, (y1, x1) =>
            //{
            //    var values = new List<float>();

            //    for (var x = 0; x < _difference[layer].ColumnCount; x++)
            //    {
            //        var outputValue = _difference[layer].GetElement(0, x)[y1, 0];
            //        values.Add(outputValue);
            //    }

            //    return values.Average() * layerGradient * _evolutionConfig.GradientConfig.WeightGradientFactor / weightColumnCount;
            //});

            var gradientBiasParams = new GradientBiasParams
            {
                Difference = _difference[layer].Cuda.DevicePointer,
                BiasGradient = Gradient[layer].CudaBias.DevicePointer,
                WeightRowCount = weightRowCount,
                InputColumnCount = input.ParametersColumnCount,
                GradientFactor = layerGradient * _evolutionConfig.GradientConfig.BiasGradientFactor * _evolutionConfig.GradientConfig.GradientFactor / weightColumnCount,
            };

            cuda.CalcGradientBias(Cuda, gradientBiasParams);

            //if (!MatrixExtension.Compare(actual, Gradient[layer].Bias))
            //{
            //    throw new Exception("Not equal!");
            //}
        }

        public void CalcExpectedOutput(CudaClient cuda, int layer)
        {
            var input = layer == 0 ? cuda.Input : Output[layer - 1];
            var weightRowCount = Layers[layer].WeightRowCount;
            var expectedRowCount = Layers[layer - 1].WeightRowCount;
            var expectedColumnCount = input.ColumnCount;
            var layerGradient = LayerGradients[layer];

            //_difference[layer].UpdateElements();

            //var actual = new ParallelMatrices(1, input.ColumnCount, (y, x) => Matrix<float>.Build.Dense(Layers[layer - 1].Weight.RowCount, 1, (y1, x1) =>
            //{
            //    var inputValue = 0.0f;
            //    for (var r = 0; r < weightRowCount; r++)
            //    {
            //        inputValue += Layers[layer].Weight[r, y1] * _difference[layer].GetElement(y, x)[r, 0];
            //    }
            //    return inputValue * _evolutionConfig.GradientConfig.InputGradientFactor * layerGradient;
            //}));

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

            //if (!ParallelMatrices.Compare(actual, _temp[layer - 1]))
            //{
            //    throw new Exception("Not equal!");
            //}
        }

        private float GetLayerGradient(int layer)
        {
            return layer == Layers.Count - 1 ? 1.0f : GetLayerGradient(layer + 1) / Layers[layer].Weight.RowCount;
        }
    }
}