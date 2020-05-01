using ManagedCuda.BasicTypes;

namespace NeuralNetwork.API.Cuda
{
    public class ReplaceData
    {
        public CUdeviceptr From;
        public CUdeviceptr To;
        public int Offset;
        public int Count;
    }
}