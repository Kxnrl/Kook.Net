using System.Text.Json.Serialization;
using Kook.Net.Converters;

namespace Kook.API;

internal class Ban
{
    [JsonPropertyName("user_id")]
    public ulong UserId { get; set; }

    [JsonPropertyName("created_time")]
    [JsonConverter(typeof(DateTimeOffsetUnixTimeMillisecondsConverter))]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("remark")]
    public string Reason { get; set; }

    [JsonPropertyName("user")]
    public User User { get; set; }
}