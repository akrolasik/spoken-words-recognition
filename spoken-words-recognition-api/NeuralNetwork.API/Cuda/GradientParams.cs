using ManagedCuda.BasicTypes;

namespace NeuralNetwork.API.Cuda
{
    public class GradientParams
    {
        public CUdeviceptr Base;
        public CUdeviceptr Gradient;
        public int Count;
    }
}