using System.Text.Json.Serialization;
using Kook.Net.Converters;

namespace Kook.API;

internal class ParagraphStruct : ElementBase
{
    [JsonPropertyName("cols")]
    public int ColumnCount { get; set; }

    [JsonPropertyName("fields")]
    public ElementBase[] Fields { get; set; }
}