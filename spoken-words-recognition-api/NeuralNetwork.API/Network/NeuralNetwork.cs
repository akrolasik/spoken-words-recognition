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

        private CudaStream _calcNeuronValues;
        public CudaStream CudaNeuronValues
        {
            get
            {
                if (_calcNeuronValues == null)
                {
                    _calcNeuronValues = new CudaStream();
                }

                return _calcNeuronValues;
            }
        }

        private CudaStream _calcExpectedDif;
        public CudaStream CudaExpectedDif
        {
            get
            {
                if (_calcExpectedDif == null)
                {
                    _calcExpectedDif = new CudaStream();
                }

                return _calcExpectedDif;
            }
        }

        private CudaStream _calcGradientWeight;
        public CudaStream CudaGradientWeight
        {
            get
            {
                if (_calcGradientWeight == null)
                {
                    _calcGradientWeight = new CudaStream();
                }

                return _calcGradientWeight;
            }
        }

        private CudaStream _calcGradientBias;
        public CudaStream CudaGradientBias
        {
            get
            {
                if (_calcGradientBias == null)
                {
                    _calcGradientBias = new CudaStream();
                }

                return _calcGradientBias;
            }
        }

        private CudaStream _calcExpectedOutput;
        public CudaStream CudaExpectedOutput
        {
            get
            {
                if (_calcExpectedOutput == null)
                {
                    _calcExpectedOutput = new CudaStream();
                }

                return _calcExpectedOutput;
            }
        }

        private CudaStream _applyGradient;
        public CudaStream CudaApplyGradient
        {
            get
            {
                if (_applyGradient == null)
                {
                    _applyGradient = new CudaStream();
                }

                return _applyGradient;
            }
        }

        private CudaStream _calcCost;
        public CudaStream CudaCost
        {
            get
            {
                if (_calcCost == null)
                {
                    _calcCost = new CudaStream();
                }

                return _calcCost;
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

        private GradientParams[] _weightGradientParams;
        private GradientParams[] _biasGradientParams;

        public void PrepareApplyGradient(CudaClient cuda)
        {
            _weightGradientParams = Enumerable.Range(0, _evolutionConfig.NetworkConfig.HiddenLayersNeuronCount.Length + 1)
                .Select(
                    layer => new GradientParams
                    {
                        Base = Layers[layer].CudaWeight.DevicePointer,
                        Gradient = Gradient[layer].CudaWeight.DevicePointer,
                        Count = Layers[layer].WeightColumnCount * Layers[layer].WeightRowCount
                    }).ToArray();

            _biasGradientParams = Enumerable.Range(0, _evolutionConfig.NetworkConfig.HiddenLayersNeuronCount.Length + 1)
                .Select(
                    layer => new GradientParams
                    {
                        Base = Layers[layer].CudaBias.DevicePointer,
                        Gradient = Gradient[layer].CudaBias.DevicePointer,
                        Count = Layers[layer].BiasColumnCount * Layers[layer].BiasRowCount
                    }).ToArray();
        }

        public void ApplyGradient(CudaClient cuda, int layer)
        {
            cuda.ApplyGradient(CudaApplyGradient, _weightGradientParams[layer]);
            cuda.ApplyGradient(CudaApplyGradient, _biasGradientParams[layer]);
        }

        public void Synchronize()
        {
            Cuda.Synchronize();

            CudaNeuronValues.Synchronize();
            CudaGradientWeight.Synchronize();
            CudaGradientBias.Synchronize();
            CudaApplyGradient.Synchronize();
            CudaExpectedOutput.Synchronize();
            CudaExpectedDif.Synchronize();
            CudaCost.Synchronize();
        }

        private NeuronValueParams[] _neuronValueParams;

        public void PrepareCalcNeuronValues(CudaClient cuda)
        {
            _neuronValueParams = Enumerable.Range(0, _evolutionConfig.NetworkConfig.HiddenLayersNeuronCount.Length + 1)
                .Select(
                    layer =>
                    {
                        var input = layer == 0 ? cuda.Input : Output[layer - 1];

                        return new NeuronValueParams
                        {
                            Weight = Layers[layer].CudaWeight.DevicePointer,
                            Bias = Layers[layer].CudaBias.DevicePointer,
                            Input = input.Cuda.DevicePointer,
                            Output = Output[layer].Cuda.DevicePointer,
                            OutputRowCount = Layers[layer].WeightRowCount,
                            OutputColumnCount = input.ParametersColumnCount,
                            CommonDim = input.ParametersRowCount,
                        };
                    }).ToArray();
        }

        public void CalcNeuronValues(CudaClient cuda, int layer)
        {
            cuda.CalcNeuronValues(CudaNeuronValues, _neuronValueParams[layer]);
        }

        private CostParams _costParams;

        public void PrepareCalcCost(CudaClient cuda)
        {
            var count = cuda.ExpectedOutput.ParametersColumnCount;

            if (_cost == null)
            {
                _cost = new float[count];
            }

            _costParams = new CostParams
            {
                Expected = cuda.ExpectedOutput.Cuda.DevicePointer,
                Actual = Output.Last().Cuda.DevicePointer,
                Cost = _cost.DevicePointer,
                ExpectedRowCount = cuda.ExpectedOutput.ParametersRowCount,
                Count = count
            };
        }

        public float[] CalcCost(CudaClient cuda)
        {
            cuda.CalcCost(CudaCost, _costParams);
            return _cost;
        }

        private ExpectedDifferenceParams[] _expectedDifferenceParams;

        public void PrepareCalcExpectedDifference(CudaClient cuda)
        {
            _expectedDifferenceParams = Enumerable.Range(0, _evolutionConfig.NetworkConfig.HiddenLayersNeuronCount.Length + 1).Select(
                layer =>
                {
                    var expected = layer == Layers.Count - 1 ? cuda.ExpectedOutput : _temp[layer];

                    return new ExpectedDifferenceParams
                    {
                        Expected = expected.Cuda.DevicePointer,
                        Actual = Output[layer].Cuda.DevicePointer,
                        Output = _difference[layer].Cuda.DevicePointer,
                        Count = expected.ParametersCount,
                    };
                }).ToArray();
        }

        public void CalcExpectedDifference(CudaClient cuda, int layer)
        {
            cuda.CalcExpectedDifference(CudaExpectedDif, _expectedDifferenceParams[layer]);
        }

        private GradientWeightParams[] _gradientWeightParams;

        public void PrepareCalcGradientWeight(CudaClient cuda)
        {
            _gradientWeightParams = Enumerable.Range(0, _evolutionConfig.NetworkConfig.HiddenLayersNeuronCount.Length + 1).Select(
                layer =>
                {
                    var input = layer == 0 ? cuda.Input : Output[layer - 1];
                    var weightRowCount = Layers[layer].WeightRowCount;
                    var weightColumnCount = Layers[layer].WeightColumnCount;
                    var layerGradient = LayerGradients[layer];

                    return new GradientWeightParams
                    {
                        Difference = _difference[layer].Cuda.DevicePointer,
                        Input = input.Cuda.DevicePointer,
                        WeightGradient = Gradient[layer].CudaWeight.DevicePointer,
                        WeightRowCount = weightRowCount,
                        WeightColumnCount = weightColumnCount,
                        InputColumnCount = input.ParametersColumnCount,
                        GradientFactor = layerGradient * _evolutionConfig.GradientConfig.WeightGradientFactor * _evolutionConfig.GradientConfig.GradientFactor,
                    };
                }).ToArray();
        }

        public void CalcGradientWeight(CudaClient cuda, int layer)
        {
            cuda.CalcGradientWeight(CudaGradientWeight, _gradientWeightParams[layer]);
        }

        private GradientBiasParams[] _gradientBiasParams;

        public void PrepareCalcGradientBias(CudaClient cuda)
        {
            _gradientBiasParams = Enumerable.Range(0, _evolutionConfig.NetworkConfig.HiddenLayersNeuronCount.Length + 1).Select(
                layer =>
                {
                    var input = layer == 0 ? cuda.Input : Output[layer - 1];
                    var weightRowCount = Layers[layer].WeightRowCount;
                    var weightColumnCount = Layers[layer].WeightColumnCount;
                    var layerGradient = LayerGradients[layer];

                    return new GradientBiasParams
                    {
                        Difference = _difference[layer].Cuda.DevicePointer,
                        BiasGradient = Gradient[layer].CudaBias.DevicePointer,
                        WeightRowCount = weightRowCount,
                        InputColumnCount = input.ParametersColumnCount,
                        GradientFactor = layerGradient * _evolutionConfig.GradientConfig.BiasGradientFactor * _evolutionConfig.GradientConfig.GradientFactor / weightColumnCount,
                    };
                }).ToArray();
        }

        public void CalcGradientBias(CudaClient cuda, int layer)
        {
            cuda.CalcGradientBias(CudaGradientBias, _gradientBiasParams[layer]);
        }

        private ExpectedOutputParams[] _expectedOutputParams;

        public void PrepareCalcExpectedOutput(CudaClient cuda)
        {
            _expectedOutputParams = Enumerable.Range(1, _evolutionConfig.NetworkConfig.HiddenLayersNeuronCount.Length).Select(
                layer =>
                {
                    var input = layer == 0 ? cuda.Input : Output[layer - 1];
                    var weightRowCount = Layers[layer].WeightRowCount;
                    var expectedRowCount = Layers[layer - 1].WeightRowCount;
                    var expectedColumnCount = input.ColumnCount;
                    var layerGradient = LayerGradients[layer];

                    return new ExpectedOutputParams
                    {
                        Difference = _difference[layer].Cuda.DevicePointer,
                        Weight = Layers[layer].CudaWeight.DevicePointer,
                        Temp = _temp[layer - 1].Cuda.DevicePointer,
                        WeightColumnCount = weightRowCount,
                        ExpectedRowCount = expectedRowCount,
                        ExpectedColumnCount = expectedColumnCount,
                        GradientFactor = layerGradient * _evolutionConfig.GradientConfig.InputGradientFactor * _evolutionConfig.GradientConfig.GradientFactor
                    };
                }).ToArray();
        }

        public void CalcExpectedOutput(CudaClient cuda, int layer)
        {
            cuda.CalcExpectedOutput(CudaExpectedOutput, _expectedOutputParams[layer - 1]);
        }

        private float GetLayerGradient(int layer)
        {
            return layer == Layers.Count - 1 ? 1.0f : GetLayerGradient(layer + 1) / Layers[layer].Weight.RowCount;
        }
    }
}