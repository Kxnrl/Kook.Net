using System.Text.Json.Serialization;

namespace Kook.API;

internal class PlainTextElement : ElementBase
{
    [JsonPropertyName("content")]
    public string Content { get; set; }
    [JsonPropertyName("emoji")]
    public bool Emoji { get; set; }
}