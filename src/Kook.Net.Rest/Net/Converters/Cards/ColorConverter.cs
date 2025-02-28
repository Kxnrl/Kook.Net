using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kook.Net.Converters;

internal class NullableColorConverter : JsonConverter<Color?>
{
    public override Color? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string hex = reader.GetString()?.TrimStart('#');
        if (string.IsNullOrWhiteSpace(hex) || hex.Length < 6)
            return null;
        return new Color(uint.Parse(hex, System.Globalization.NumberStyles.HexNumber));
    }

    public override void Write(Utf8JsonWriter writer, Color? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.HasValue ? $"#{value.Value.RawValue:X6}" : string.Empty);
    }
}