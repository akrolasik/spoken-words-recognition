using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace NeuralNetwork.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();

            //return;

            //const int iterations = 100;
            //const int threads = 10;
            //const int operationCount = 10;

            //const int commonDimension = 8500;

            //const int inputMatrixCount = 25;
            //const int inputMatrixWidth = 1;
            //const int inputMatrixHeight = commonDimension;

            //const int unitMatrixCount = 1;
            //const int unitMatrixWidth = commonDimension;
            //const int unitMatrixHeight = 400;

            //var random = new Random();

            ////=========================================================

            //var unitMatrices = new ParallelMatrices(unitMatrixCount, 1, (y, x) =>
            //    Matrix<float>.Build.Dense(unitMatrixHeight, unitMatrixWidth,
            //        (y1, x1) => (float)random.NextDouble()));

            //var inputMatrices = new ParallelMatrices(1, inputMatrixCount, (y, x) =>
            //    Matrix<float>.Build.Dense(inputMatrixHeight, inputMatrixWidth,
            //        (y1, x1) => (float) random.NextDouble()));

            //var outputMatrices = Enumerable.Range(0, threads).Select(x => new ParallelMatrices(unitMatrixCount, inputMatrixCount, (y, x) =>
            //    Matrix<float>.Build.Dense(unitMatrixHeight, inputMatrixWidth,
            //        (y1, x1) => 0.0f))).ToList();

            ////var outputTest1 = ParallelMatrices.Multiply(unitMatrices, inputMatrices).Parameters;
            ////var outputTest2 = ParallelMatrices.MultiplyExplicit(unitMatrices, inputMatrices).Parameters;

            ////for (var i = 0; i < outputTest1.Length; i++)
            ////{
            ////    if (Math.Abs(outputTest1[i] - outputTest2[i]) > 0.0001f)
            ////    {
            ////        Console.WriteLine(i);
            ////    }
            ////}

            ////=========================================================

            //var kernelName = "parallelMatricesMultiplication";
            //var filename = $"{kernelName}.txt";
            //var fileToCompile = File.ReadAllText(filename);
            //var rtc = new CudaRuntimeCompiler(fileToCompile, kernelName);

            //rtc.Compile(new string[] { });
            //var ptx = rtc.GetPTX();
            //rtc.Dispose();

            //var ctx = new CudaContext(true);
            //var parallelMatricesMultiplication = ctx.LoadKernelPTX(ptx, kernelName);

            ////=========================================================

            //var unitParameters = (CudaDeviceVariable<float>) unitMatrices.Parameters;
            //var inputParameters = (CudaDeviceVariable<float>) inputMatrices.Parameters;
            //var outputParameters = outputMatrices.Select(x => (CudaDeviceVariable<float>) x.Parameters).ToList();

            //var threadsPerBlock = 1024;
            //var blocksPerGrid = (outputMatrices[0].ParametersCount + threadsPerBlock - 1) / threadsPerBlock;

            //parallelMatricesMultiplication.BlockDimensions = new dim3(threadsPerBlock, 1, 1);
            //parallelMatricesMultiplication.GridDimensions = new dim3(blocksPerGrid, 1, 1);

            //var streams = Enumerable.Range(0, threads).ToDictionary(x => x, x => new CudaStream());

            ////var stopEvent = new CudaEvent(CUEventFlags.BlockingSync);

            //var stopwatch = new Stopwatch();
            //stopwatch.Start();

            //for (var i = 0; i < iterations; i++)
            //{
            //    foreach (var stream in streams)
            //    {
            //        parallelMatricesMultiplication.RunAsync(stream.Value.Stream,
            //            unitParameters.DevicePointer,
            //            inputParameters.DevicePointer,
            //            outputParameters[stream.Key].DevicePointer,
            //            unitMatrices.ParametersRowCount,
            //            inputMatrices.ParametersColumnCount,
            //            unitMatrices.ParametersColumnCount);
            //    }

            //    foreach (var stream in streams)
            //    {
            //        stream.Value.Synchronize();
            //    }
            //}

            //stopwatch.Stop();

            //var average = stopwatch.Elapsed / iterations / threads * operationCount;

            //var generationsPerMinute = (int)(TimeSpan.FromMinutes(1) / average);
            //var generationsPerHour = (int)(TimeSpan.FromHours(1) / average);

            ////=========================================================

            //foreach (var cudaStream in streams)
            //{
            //    cudaStream.Value.Dispose();
            //}

            //unitParameters.Dispose();
            //inputParameters.Dispose();
            //foreach (var cudaDeviceVariable in outputParameters)
            //{
            //    cudaDeviceVariable.Dispose();
            //}

            //ctx.Dispose();

            
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
