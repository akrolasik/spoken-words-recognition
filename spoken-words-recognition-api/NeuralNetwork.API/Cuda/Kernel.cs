using System.IO;
using ManagedCuda;
using ManagedCuda.NVRTC;

namespace NeuralNetwork.API.Cuda
{
    public class Kernel
    {
        public readonly CudaKernel Cuda;

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
    }
}