using System.Text.Json.Serialization;
using Kook.Net.Converters;

namespace Kook.API;

internal class ActionGroupModule : ModuleBase
{
    [JsonPropertyName("elements")]
    public ButtonElement[] Elements { get; set; }
}