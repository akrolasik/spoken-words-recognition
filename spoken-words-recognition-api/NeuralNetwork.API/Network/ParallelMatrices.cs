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

        public int ElementRowCount => GetElement(0, 0).RowCount;
        public int ElementColumnCount => GetElement(0, 0).ColumnCount;
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
                            parameters.AddRange(GetElement(y, x).Column(c));
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
        }

        public ParallelMatrices(int rowCount, int columnCount, List<Matrix<float>> elements)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;
            Elements = elements;
        }

        public void UpdateElements()
        {
            if (_cuda != null)
            {
                _parameters = _cuda;
            }
        }

        public Matrix<float> GetElement(int y, int x)
        {
            return Elements[y * ColumnCount + x];
        }

        internal static bool Compare(ParallelMatrices output1, ParallelMatrices output2)
        {
            var params1 = output1.Parameters;
            var params2 = output2.Parameters;

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


        //public void Increment(ParallelMatrices other)
        //{
        //    for (var y = 0; y < RowCount; y++)
        //    {
        //        for (var x = 0; x < ColumnCount; x++)
        //        {
        //            Elements[y * ColumnCount + x] += other.GetElement(y, x);
        //        }
        //    }
        //}

        //public void Decrement(ParallelMatrices other)
        //{
        //    for (var y = 0; y < RowCount; y++)
        //    {
        //        for (var x = 0; x < ColumnCount; x++)
        //        {
        //            Elements[y * ColumnCount + x] += other.GetElement(y, x);
        //        }
        //    }
        //}

        //public static Matrix<float> DefaultMultiplicationResult(Matrix<float> a, Matrix<float> b)
        //{
        //    return Matrix<float>.Build.Dense(a.RowCount, b.ColumnCount);
        //}

        //public static ParallelMatrices Multiply(ParallelMatrices a, ParallelMatrices b)
        //{
        //    if (a.ColumnCount != b.RowCount)
        //    {
        //        throw new ArgumentException();
        //    }

        //    return new ParallelMatrices(a.RowCount, b.ColumnCount, (y, x) =>
        //    {
        //        var element = DefaultMultiplicationResult(a.DefaultElement, b.DefaultElement);

        //        for (var i = 0; i < a.ColumnCount; i++)
        //        {
        //            element += a.GetElement(y, i) * b.GetElement(i, x);
        //        }

        //        return element;
        //    });
        //}

        //public void SetParameters(float[] parameters)
        //{
        //    for (var y = 0; y < RowCount; y++)
        //    {
        //        for (var x = 0; x < ColumnCount; x++)
        //        {
        //            var skip = x * ElementColumnCount * ParametersRowCount + y * ElementRowCount;

        //            var array = Enumerable.Range(0, ElementColumnCount).SelectMany(i =>
        //                parameters.Skip(skip + i * ParametersRowCount).Take(ElementRowCount)).ToArray();

        //            Elements[y * ColumnCount + x] = Matrix<float>.Build.Dense(ElementRowCount, ElementColumnCount, array);
        //        }
        //    }
        //}

        //public static ParallelMatrices MultiplyExplicit(ParallelMatrices a, ParallelMatrices b)
        //{
        //    if (a.ParametersColumnCount != b.ParametersRowCount)
        //    {
        //        throw new ArgumentException();
        //    }

        //    var parametersA = a.Parameters;
        //    var parametersB = b.Parameters;

        //    var outputRowCount = a.ParametersRowCount;
        //    var outputColumnCount = b.ParametersColumnCount;
        //    var commonDim = a.ParametersColumnCount;

        //    var parameters = new float[outputRowCount * outputColumnCount];

        //    for (var index = 0; index < parameters.Length; index++)
        //    {
        //        var sum = 0.0f;

        //        for (var i = 0; i < commonDim; i++)
        //        {
        //            var xa = i;
        //            var ya = index % outputRowCount;

        //            var xb = index / outputRowCount;
        //            var yb = i;

        //            var ia = xa * outputRowCount + ya;
        //            var ib = xb * commonDim + yb;

        //            sum += parametersA[ia] * parametersB[ib];
        //        }

        //        parameters[index] = sum;
        //    }

        //    var result = new ParallelMatrices(a.RowCount, b.ColumnCount, (y, x) => Matrix<float>.Build.Dense(a.ElementRowCount, b.ElementColumnCount));

        //    result.SetParameters(parameters);

        //    return result;
        //}

    }
}