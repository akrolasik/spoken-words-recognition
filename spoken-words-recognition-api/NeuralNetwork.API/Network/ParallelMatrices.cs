using System;
using System.Collections.Generic;
using System.Linq;
using ManagedCuda;
using MathNet.Numerics.LinearAlgebra;

namespace NeuralNetwork.API.Network
{
    public class ParallelMatrices
    {
        public int RowCount;
        public int ColumnCount;

        public int ElementRowCount;
        public int ElementColumnCount;
        public int ElementsParametersCount => ElementRowCount * ElementColumnCount;

        public int ParametersRowCount => RowCount * ElementRowCount;
        public int ParametersColumnCount => ColumnCount * ElementColumnCount;
        public int ParametersCount => ParametersRowCount * ParametersColumnCount;

        public Matrix<float> DefaultElement => Matrix<float>.Build.Dense(ElementRowCount, ElementColumnCount);

        public List<Matrix<float>> Elements;

        private CudaDeviceVariable<float> _cuda;

        public CudaDeviceVariable<float> Cuda
        {
            get
            {
                if (_cuda == null)
                {
                    _cuda = _parameters;
                }

                return _cuda;
            }
        }

        public float[] Parameters
        {
            get
            {
                if (_cuda != null)
                {
                    _parameters = _cuda;
                }

                return _parameters;
            }

            set
            {
                if (_cuda != null)
                {
                    _cuda.Dispose();
                    _cuda = null;
                }

                _parameters = value;
            }
        }

        private float[] _parameters
        {
            get
            {
                var parameters = new List<float>();

                for (var x = 0; x < ColumnCount; x++)
                {
                    for (var c = 0; c < ElementColumnCount; c++)
                    {
                        for (var y = 0; y < RowCount; y++)
                        {
                            parameters.AddRange(Elements[y * ColumnCount + x].Column(c));
                        }
                    }
                }

                return parameters.ToArray();
            }

            set
            {
                for (var y = 0; y < RowCount; y++)
                {
                    for (var x = 0; x < ColumnCount; x++)
                    {
                        var skip = x * ElementColumnCount * ParametersRowCount + y * ElementRowCount;

                        var array = Enumerable.Range(0, ElementColumnCount).SelectMany(i =>
                            value.Skip(skip + i * ParametersRowCount).Take(ElementRowCount)).ToArray();

                        Elements[y * ColumnCount + x] = Matrix<float>.Build.Dense(ElementRowCount, ElementColumnCount, array);
                    }
                }
            }
        }

        public ParallelMatrices(int rowCount, int columnCount, Func<int, int, Matrix<float>> init)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;

            Elements = new List<Matrix<float>>();

            for (var y = 0; y < rowCount; y++)
            {
                for (var x = 0; x < columnCount; x++)
                {
                    Elements.Add(init(y, x));
                }
            }

            ElementRowCount = Elements.First().RowCount;
            ElementColumnCount = Elements.First().ColumnCount;
        }

        public ParallelMatrices(int rowCount, int columnCount, List<Matrix<float>> elements)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;
            Elements = elements;

            ElementRowCount = Elements.First().RowCount;
            ElementColumnCount = Elements.First().ColumnCount;
        }

        public Matrix<float> GetElement(int y, int x)
        {
            var skip = x * ElementColumnCount * ParametersRowCount + y * ElementRowCount;

            var array = Enumerable.Range(0, ElementColumnCount).SelectMany(i =>
                ((float[])_cuda).Skip(skip + i * ParametersRowCount).Take(ElementRowCount)).ToArray();

            return Matrix<float>.Build.Dense(ElementRowCount, ElementColumnCount, array);
        }

        public void Dispose()
        {
            _cuda?.Dispose();
        }
    }
}