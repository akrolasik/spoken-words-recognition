using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using NeuralNetwork.API.Network;

namespace NeuralNetwork.API.Extensions
{
    public static class ListExtension
    {
        public static NeuralNetworkResult Average(this List<NeuralNetworkResult> list)
        {
            return new NeuralNetworkResult
            {
                Cost = list.Average(x => x.Cost),
                Gradient = new NeuralNetworkGradient
                {
                    Layers = Enumerable.Range(0, list.First().Gradient.Layers.Count).Select(i =>
                    {
                        var weight = list.First().Gradient.Layers[i].Weight;
                        var bias = list.First().Gradient.Layers[i].Bias;

                        return new MatrixFunctionGradient
                        {
                            Weight = Matrix<double>.Build.Dense(weight.RowCount, weight.ColumnCount,
                                (y, x) => list.Average(result => result.Gradient.Layers[i].Weight[y, x])),

                            Bias = Matrix<double>.Build.Dense(bias.RowCount, bias.ColumnCount,
                                (y, x) => list.Average(result => result.Gradient.Layers[i].Bias[y, x]))
                        };
                    }).ToList()
                }
            };
        }
    }
}