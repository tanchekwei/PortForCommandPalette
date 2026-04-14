using System.Text.Json.Serialization;

namespace PortForCommandPalette.Classes
{
    [JsonSerializable(typeof(PortInfo))]
    internal sealed partial class PortInfoSerializerContext : JsonSerializerContext
    {
    }
}
