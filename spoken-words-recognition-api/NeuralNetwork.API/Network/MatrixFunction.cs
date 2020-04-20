using System;
using MathNet.Numerics.LinearAlgebra;

namespace NeuralNetwork.API.Network
{
    public class MatrixFunction
    {
        private static readonly Random Random = new Random();
        
        public Matrix<double> Weight;
        public Matrix<double> Bias;

        public MatrixFunction(int rows, int columns)
        {
            Weight = Matrix<double>.Build.Dense(rows, columns, (y, x) => GetRandom());
            Bias = Matrix<double>.Build.Dense(rows, 1, (y, x) => GetRandom());
        }

        public MatrixFunction(Matrix<double> weight, Matrix<double> bias)
        {
            Weight = weight;
            Bias = bias;
        }

        public Matrix<double> Calculate(Matrix<double> input)
        {
            var temp = Weight * input + Bias;
            return Matrix<double>.Build.Dense(temp.RowCount, temp.ColumnCount, (y, x) => 1.0 / (1.0 + (double)Math.Exp(-temp[y, x])));
            //return Matrix<double>.Build.Dense(temp.RowCount, temp.ColumnCount, (y, x) => temp[y, x]);
        }

        private double GetRandom()
        {
            lock (Random)
            {
                return (double)(Random.NextDouble() * 2 - 1);
            }
        }

    }
}