using System;
using ManagedCuda;
using MathNet.Numerics.LinearAlgebra;

namespace NeuralNetwork.API.Network
{
    public class MatrixExtension
    {
        public static bool Compare(Matrix<float> matrix1, Matrix<float> matrix2)
        {
            var params1 = matrix1.AsColumnMajorArray();
            var params2 = matrix2.AsColumnMajorArray();

            if (params1.Length != params2.Length)
            {
                return false;
            }

            for (var i = 0; i < params1.Length; i++)
            {
                if (Math.Abs(params1[i] - params2[i]) > 1.0E-5f)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public class MatrixFunction
    {
        private static readonly Random Random = new Random();

        public int WeightRowCount;
        public int WeightColumnCount;
        public int BiasRowCount;
        public int BiasColumnCount;

        public Matrix<float> Weight
        {
            get
            {
                if (_cudaWeight != null)
                {
                    _weight = Matrix<float>.Build.Dense(_weight.RowCount, _weight.ColumnCount, (float[]) _cudaWeight);
                }

                return _weight;
            }
            set
            {
                if (_cudaWeight != null)
                {
                    _cudaWeight.Dispose();
                    _cudaWeight = null;
                }

                _weight = value;
            }
        }

        public Matrix<float> Bias
        {
            get
            {
                if (_cudaBias != null)
                {
                    _bias = Matrix<float>.Build.Dense(_bias.RowCount, _bias.ColumnCount, (float[])_cudaBias);
                }

                return _bias;
            }
            set
            {
                if (_cudaBias != null)
                {
                    _cudaBias.Dispose();
                    _cudaBias = null;
                }

                _bias = value;
            }
        }

        public CudaDeviceVariable<float> CudaWeight
        {
            get
            {
                if (_cudaWeight == null)
                {
                    _cudaWeight = Weight.ToColumnMajorArray();
                }

                return _cudaWeight;
            }
        }

        public CudaDeviceVariable<float> CudaBias
        {
            get
            {
                if (_cudaBias == null)
                {
                    _cudaBias = Bias.ToColumnMajorArray();
                }

                return _cudaBias;
            }
        }

        private CudaDeviceVariable<float> _cudaWeight;
        private CudaDeviceVariable<float> _cudaBias;
        private Matrix<float> _weight;
        private Matrix<float> _bias;

        public MatrixFunction()
        {

        }

        public MatrixFunction(int rows, int columns)
        {
            Weight = Matrix<float>.Build.Dense(rows, columns, (y, x) => GetRandom());
            Bias = Matrix<float>.Build.Dense(rows, 1, (y, x) => GetRandom());

            WeightRowCount = Weight.RowCount;
            WeightColumnCount = Weight.ColumnCount;
            BiasRowCount = Bias.RowCount;
            BiasColumnCount = Bias.ColumnCount;
        }

        public MatrixFunction(Matrix<float> weight, Matrix<float> bias)
        {
            Weight = weight;
            Bias = bias;

            WeightRowCount = Weight.RowCount;
            WeightColumnCount = Weight.ColumnCount;
            BiasRowCount = Bias.RowCount;
            BiasColumnCount = Bias.ColumnCount;
        }

        public Matrix<float> Calculate(Matrix<float> input)
        {
            var temp = Weight * input + Bias;
            return Matrix<float>.Build.Dense(temp.RowCount, temp.ColumnCount, (y, x) => 1.0f / (1.0f + (float)Math.Exp(-temp[y, x])));
            //return Matrix<float>.Build.Dense(temp.RowCount, temp.ColumnCount, (y, x) => temp[y, x]);
        }

        private float GetRandom()
        {
            lock (Random)
            {
                return (float)(Random.NextDouble() * 2 - 1);
            }
        }

    }
}