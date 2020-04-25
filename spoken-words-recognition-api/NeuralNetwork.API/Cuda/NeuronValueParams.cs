using ManagedCuda.BasicTypes;

namespace NeuralNetwork.API.Cuda
{
    public class NeuronValueParams
    {
        public CUdeviceptr Weight;
        public CUdeviceptr Bias;
        public CUdeviceptr Input;
        public CUdeviceptr Output;
        public int OutputRowCount;
        public int OutputColumnCount;
        public int CommonDim;
    }
}