using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kook.Net.Converters;

internal class DateTimeOffsetUnixTimeSecondsConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64());
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        if (value == DateTimeOffset.MinValue || value == DateTimeOffset.UnixEpoch)
            writer.WriteNullValue();
        else
            writer.WriteNumberValue(value.ToUnixTimeSeconds());
    }
}