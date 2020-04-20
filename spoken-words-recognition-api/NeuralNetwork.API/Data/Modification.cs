using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NeuralNetwork.API.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Modification
    {
        None,
        Faster,
        Slower,
        Louder,
        Quieter,
        Noise,
        Max
    }
}