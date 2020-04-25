using ManagedCuda.BasicTypes;

namespace NeuralNetwork.API.Cuda
{
    public class CostParams
    {
        public CUdeviceptr Expected;
        public CUdeviceptr Actual;
        public CUdeviceptr Cost;
        public int ExpectedRowCount;
        public int Count;
    }
}