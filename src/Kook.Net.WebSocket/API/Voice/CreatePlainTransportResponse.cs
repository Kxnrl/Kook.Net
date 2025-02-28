using System.Text.Json.Serialization;

namespace Kook.API.Voice;

internal class CreatePlainTransportResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    [JsonPropertyName("ip")]
    public string Ip { get; set; }
    
    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("rtcpPort")]
    public string RTCPPort { get; set; }
}