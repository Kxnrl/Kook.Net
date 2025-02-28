using System.Text.Json.Serialization;
using Kook.Net.Converters;

namespace Kook.API;

internal class ImageElement : ElementBase
{
    [JsonPropertyName("src")] public string Source { get; set; }
    [JsonPropertyName("alt")] public string Alternative { get; set; }

    [JsonPropertyName("size")]
    [JsonConverter(typeof(ImageSizeConverter))]
    public ImageSize? Size { get; set; }

    [JsonPropertyName("circle")] public bool? Circle { get; set; }
}