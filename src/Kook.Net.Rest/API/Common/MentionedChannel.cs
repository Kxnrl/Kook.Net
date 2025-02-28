using System.Text.Json.Serialization;
using Kook.Net.Converters;

namespace Kook.API;

internal class MentionedChannel
{
    [JsonPropertyName("id")]
    public ulong Id { get; set; }
    [JsonPropertyName("guild_id")]
    public ulong GuildId { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
}