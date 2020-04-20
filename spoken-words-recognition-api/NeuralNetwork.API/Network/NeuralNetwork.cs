using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using NeuralNetwork.API.Config;
using NeuralNetwork.API.Data;

namespace NeuralNetwork.API.Network
{
    public class NeuralNetwork
    {
        public List<MatrixFunction> Layers;

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
        }

        public NeuralNetwork(EvolutionConfig evolutionConfig, List<MatrixFunction> layers)
        {
            _evolutionConfig = evolutionConfig;
            Layers = layers;
        }

        public NeuralNetworkResult Calculate(TrainingData trainingData)
        {
            var neuronValues = new List<Matrix<double>>();

            for (var i = 0; i < Layers.Count; i++)
            {
                var input = i == 0 ? trainingData.Input() : neuronValues[i - 1];
                neuronValues.Add(Layers[i].Calculate(input));
            }

            var output = neuronValues.Last().ToColumnMajorArray().ToArray();
            var expectedOutput = trainingData.ExpectedOutput.ToColumnMajorArray();

            var cost = CalculateCost(output, expectedOutput);
            var gradient = CalculateGradient(neuronValues, trainingData);

            return new NeuralNetworkResult
            {
                Output = output,
                Cost = cost,
                Gradient = gradient
            };
        }

        private double CalculateCost(double[] output, double[] expectedOutput)
        {
            return (double) Enumerable.Range(0, output.Length)
                .Sum(x => Math.Pow(Math.Abs(output[x] - expectedOutput[x]), 2));
        }

        private double GetLayerGradient(List<MatrixFunction> layers, int layer)
        {
            return layer == layers.Count - 1 ? 1.0 : GetLayerGradient(layers, layer + 1) / layers[layer].Weight.RowCount;
        }

        private NeuralNetworkGradient CalculateGradient(List<Matrix<double>> neuronValues, TrainingData trainingData)
        {
            var neuralNetworkGradient = new NeuralNetworkGradient
            {
                Layers = new List<MatrixFunctionGradient>()
            };

            Matrix<double> temp = null;

            for (var i = Layers.Count - 1; i >= 0; i--)
            {
                var layerIndex = i;
                var widthRowCount = Layers[i].Weight.RowCount;
                var widthColumnCount = Layers[i].Weight.ColumnCount;

                var expected = layerIndex == Layers.Count - 1 ? trainingData.ExpectedOutput : temp;
                var difference = expected - neuronValues[layerIndex];
                var layerGradient = GetLayerGradient(Layers, layerIndex);

                neuralNetworkGradient.Layers.Insert(0, new MatrixFunctionGradient
                {
                    Weight = Matrix<double>.Build.Dense(widthRowCount, widthColumnCount, (y, x) =>
                    {
                        var outputValue = difference[y, 0];
                        var inputValue = (layerIndex == 0 ? trainingData.Input() : neuronValues[layerIndex - 1])[x, 0];
                        return outputValue * inputValue * _evolutionConfig.GradientConfig.WeightGradientFactor * layerGradient;
                    }),
                    Bias = Matrix<double>.Build.Dense(widthRowCount, 1, (y, x) =>
                    {
                        var outputValue = difference[y, 0];
                        return outputValue * _evolutionConfig.GradientConfig.BiasGradientFactor * layerGradient / widthRowCount;
                    })
                });

                if (layerIndex > 0)
                {
                    temp = Matrix<double>.Build.Dense(Layers[layerIndex - 1].Weight.RowCount, 1, (y, x) =>
                    {
                        var inputValue = 0.0;
                        for (var r = 0; r < widthRowCount; r++)
                        {
                            inputValue += Layers[layerIndex].Weight[r, y] * difference[r, 0];
                        }
                        return inputValue * _evolutionConfig.GradientConfig.InputGradientFactor * layerGradient;
                    });
                }
            }

            return neuralNetworkGradient;
        }

        
    }
}