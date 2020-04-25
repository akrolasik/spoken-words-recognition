using System.IO;
using ManagedCuda;
using ManagedCuda.NVRTC;
using ManagedCuda.VectorTypes;

namespace NeuralNetwork.API.Cuda
{
    public class Kernel
    {
        public readonly CudaKernel Cuda;

        private const int ThreadsPerBlock = 1024;

        public Kernel(CudaContext ctx, string name)
        {
            var filename = $"Kernel/{name}.txt";
            var fileToCompile = File.ReadAllText(filename);
            var rtc = new CudaRuntimeCompiler(fileToCompile, name);

            rtc.Compile(new string[] { });
            var ptx = rtc.GetPTX();
            rtc.Dispose();
            
            Cuda = ctx.LoadKernelPTX(ptx, name);
        }

        public void Init(int outputParameterCount)
        {
            var blocksPerGrid = (outputParameterCount + ThreadsPerBlock - 1) / ThreadsPerBlock;

            Cuda.BlockDimensions = new dim3(ThreadsPerBlock, 1, 1);
            Cuda.GridDimensions = new dim3(blocksPerGrid, 1, 1);
        }
    }
}