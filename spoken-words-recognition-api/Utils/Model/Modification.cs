using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Utils.Model
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