using ManagedCuda.BasicTypes;

namespace NeuralNetwork.API.Cuda
{
    public class GradientBiasParams
    {
        public CUdeviceptr Difference;
        public CUdeviceptr BiasGradient;
        public int WeightRowCount;
        public int InputColumnCount;
        public float GradientFactor;
    }
}