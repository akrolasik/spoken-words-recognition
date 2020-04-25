using ManagedCuda.BasicTypes;

namespace NeuralNetwork.API.Cuda
{
    public class ExpectedOutputParams
    {
        public CUdeviceptr Difference;
        public CUdeviceptr Weight;
        public CUdeviceptr Temp;
        public int WeightColumnCount;
        public int ExpectedRowCount;
        public int ExpectedColumnCount;
        public float GradientFactor;
    }
}