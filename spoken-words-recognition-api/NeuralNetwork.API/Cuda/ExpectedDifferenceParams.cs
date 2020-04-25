using ManagedCuda.BasicTypes;

namespace NeuralNetwork.API.Cuda
{
    public class ExpectedDifferenceParams
    {
        public CUdeviceptr Expected;
        public CUdeviceptr Actual;
        public CUdeviceptr Output;
        public int Count;
    }
}