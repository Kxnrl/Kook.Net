using System.Text.Json.Serialization;
using Kook.Net.Converters;

namespace Kook.API.Gateway;

internal class GatewaySocketFrame
{
    [JsonPropertyName("s")]
    [JsonConverter(typeof(GatewaySocketFrameTypeConverter))]
    public GatewaySocketFrameType Type { get; set; }

    [JsonPropertyName("sn")]
    public int? Sequence { get; set; }

    [JsonPropertyName("d")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object Payload { get; set; }
}