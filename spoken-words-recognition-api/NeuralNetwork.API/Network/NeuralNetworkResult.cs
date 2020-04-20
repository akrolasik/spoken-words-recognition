namespace NeuralNetwork.API.Network
{
    public class NeuralNetworkResult
    {
        public double Cost;
        public NeuralNetworkGradient Gradient;
        public double[] Output;
    }
}