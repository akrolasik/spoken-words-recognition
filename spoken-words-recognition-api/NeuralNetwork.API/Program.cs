using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ManagedCuda;
using ManagedCuda.NVRTC;
using ManagedCuda.VectorTypes;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace NeuralNetwork.API
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

        public float[] Parameters
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

        public Matrix<float> GetElement(int y, int x)
        {
            return Elements[y * ColumnCount + x];
        }

        public static Matrix<float> DefaultMultiplicationResult(Matrix<float> a, Matrix<float> b)
        {
            return Matrix<float>.Build.Dense(a.RowCount, b.ColumnCount);
        }

        public static ParallelMatrices Multiply(ParallelMatrices a, ParallelMatrices b)
        {
            if (a.ColumnCount != b.RowCount)
            {
                throw new ArgumentException();
            }

            return new ParallelMatrices(a.RowCount, b.ColumnCount, (y, x) =>
            {
                var element = DefaultMultiplicationResult(a.DefaultElement, b.DefaultElement);

                for (var i = 0; i < a.ColumnCount; i++)
                {
                    element += a.GetElement(y, i) * b.GetElement(i, x);
                }

                return element;
            });
        }

        public static ParallelMatrices MultiplyExplicit(ParallelMatrices a, ParallelMatrices b)
        {
            if (a.ParametersColumnCount != b.ParametersRowCount)
            {
                throw new ArgumentException();
            }

            var parametersA = a.Parameters;
            var parametersB = b.Parameters;

            var outputRowCount = a.ParametersRowCount;
            var outputColumnCount = b.ParametersColumnCount;
            var commonDim = a.ParametersColumnCount;

            var c = new float[outputRowCount * outputColumnCount];

            for (var index = 0; index < c.Length; index++)
            {
                var sum = 0.0f;

                for (var i = 0; i < commonDim; i++)
                {
                    var xa = i;
                    var ya = index % outputRowCount;

                    var xb = index / outputRowCount;
                    var yb = i;

                    var ia = xa * outputRowCount + ya;
                    var ib = xb * commonDim + yb;

                    sum += parametersA[ia] * parametersB[ib];
                }

                c[index] = sum;
            }

            var result = new ParallelMatrices(a.RowCount, b.ColumnCount, (y, x) => Matrix<float>.Build.Dense(a.ElementRowCount, b.ElementColumnCount));

            result.Parameters = c;

            return result;
        }

    }

    public class Program
    {
        public static void Main(string[] args)
        {
            const int iterations = 100;
            const int threads = 10;
            const int operationCount = 10;

            const int commonDimension = 8500;

            const int inputMatrixCount = 25;
            const int inputMatrixWidth = 1;
            const int inputMatrixHeight = commonDimension;

            const int unitMatrixCount = 1;
            const int unitMatrixWidth = commonDimension;
            const int unitMatrixHeight = 400;

            var random = new Random();

            //=========================================================

            var unitMatrices = new ParallelMatrices(unitMatrixCount, 1, (y, x) =>
                Matrix<float>.Build.Dense(unitMatrixHeight, unitMatrixWidth,
                    (y1, x1) => (float)random.NextDouble()));

            var inputMatrices = new ParallelMatrices(1, inputMatrixCount, (y, x) =>
                Matrix<float>.Build.Dense(inputMatrixHeight, inputMatrixWidth,
                    (y1, x1) => (float) random.NextDouble()));

            var outputMatrices = Enumerable.Range(0, threads).Select(x => new ParallelMatrices(unitMatrixCount, inputMatrixCount, (y, x) =>
                Matrix<float>.Build.Dense(unitMatrixHeight, inputMatrixWidth,
                    (y1, x1) => 0.0f))).ToList();

            //var outputTest1 = ParallelMatrices.Multiply(unitMatrices, inputMatrices).Parameters;
            //var outputTest2 = ParallelMatrices.MultiplyExplicit(unitMatrices, inputMatrices).Parameters;

            //for (var i = 0; i < outputTest1.Length; i++)
            //{
            //    if (Math.Abs(outputTest1[i] - outputTest2[i]) > 0.0001f)
            //    {
            //        Console.WriteLine(i);
            //    }
            //}

            //=========================================================

            var kernelName = "parallelMatricesMultiplication";
            var filename = $"{kernelName}.txt";
            var fileToCompile = File.ReadAllText(filename);
            var rtc = new CudaRuntimeCompiler(fileToCompile, kernelName);

            rtc.Compile(new string[] { });
            var ptx = rtc.GetPTX();
            rtc.Dispose();

            var ctx = new CudaContext(true);
            var parallelMatricesMultiplication = ctx.LoadKernelPTX(ptx, kernelName);

            //=========================================================

            var unitParameters = (CudaDeviceVariable<float>) unitMatrices.Parameters;
            var inputParameters = (CudaDeviceVariable<float>) inputMatrices.Parameters;
            var outputParameters = outputMatrices.Select(x => (CudaDeviceVariable<float>) x.Parameters).ToList();

            var threadsPerBlock = 1024;
            var blocksPerGrid = (outputMatrices[0].ParametersCount + threadsPerBlock - 1) / threadsPerBlock;

            parallelMatricesMultiplication.BlockDimensions = new dim3(threadsPerBlock, 1, 1);
            parallelMatricesMultiplication.GridDimensions = new dim3(blocksPerGrid, 1, 1);

            var streams = Enumerable.Range(0, threads).ToDictionary(x => x, x => new CudaStream());

            //var stopEvent = new CudaEvent(CUEventFlags.BlockingSync);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (var i = 0; i < iterations; i++)
            {
                foreach (var stream in streams)
                {
                    parallelMatricesMultiplication.RunAsync(stream.Value.Stream,
                        unitParameters.DevicePointer,
                        inputParameters.DevicePointer,
                        outputParameters[stream.Key].DevicePointer,
                        unitMatrices.ParametersRowCount,
                        inputMatrices.ParametersColumnCount,
                        unitMatrices.ParametersColumnCount);
                }

                foreach (var stream in streams)
                {
                    stream.Value.Synchronize();
                }
            }

            stopwatch.Stop();

            var average = stopwatch.Elapsed / iterations / threads * operationCount;

            var generationsPerMinute = (int)(TimeSpan.FromMinutes(1) / average);
            var generationsPerHour = (int)(TimeSpan.FromHours(1) / average);

            //=========================================================

            foreach (var cudaStream in streams)
            {
                cudaStream.Value.Dispose();
            }

            unitParameters.Dispose();
            inputParameters.Dispose();
            foreach (var cudaDeviceVariable in outputParameters)
            {
                cudaDeviceVariable.Dispose();
            }

            ctx.Dispose();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
