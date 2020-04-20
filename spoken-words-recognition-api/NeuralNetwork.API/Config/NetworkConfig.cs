namespace NeuralNetwork.API.Config
{
    public class NetworkConfig
    {
        public InputResolution InputResolution { get; set; }
        public int[] HiddenLayersNeuronCount { get; set; }
        public int OutputCount { get; set; }
        public int InputCount => InputResolution.Width * InputResolution.Height;

        public int GetLayerWeightWidth(int layer)
        {
            return layer == 0 ? InputCount : HiddenLayersNeuronCount[layer - 1];
        }

        public int GetLayerWeightParamsCount(int layer)
        {
            var width = GetLayerWeightWidth(layer);
            var height = GetLayerWeightHeight(layer);
            return width * height;
        }

        public int GetLayerWeightHeight(int layer)
        {
            return layer == HiddenLayersNeuronCount.Length ? OutputCount : HiddenLayersNeuronCount[layer];
        }
    }
}