using ManagedCuda.BasicTypes;

namespace NeuralNetwork.API.Cuda
{
    public class GradientWeightParams
    {
        public CUdeviceptr Difference;
        public CUdeviceptr Input;
        public CUdeviceptr WeightGradient;
        public int WeightRowCount;
        public int WeightColumnCount;
        public int InputColumnCount;
        public float GradientFactor;
    }
}