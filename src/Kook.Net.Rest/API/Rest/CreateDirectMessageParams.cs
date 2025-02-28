using System.Text.Json.Serialization;
using Kook.Net.Converters;

namespace Kook.API.Rest;

internal class CreateDirectMessageParams
{
    [JsonPropertyName("type")] public MessageType Type { get; set; }

    [JsonPropertyName("target_id")]
    [JsonConverter(typeof(NullableUInt64Converter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? UserId { get; set; }

    [JsonPropertyName("chat_code")]
    [JsonConverter(typeof(NullableChatCodeConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? ChatCode { get; set; }

    [JsonPropertyName("content")] public string Content { get; set; }

    [JsonPropertyName("quote")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? QuotedMessageId { get; set; }

    [JsonPropertyName("nonce")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Nonce { get; set; }

    public CreateDirectMessageParams(MessageType messageType, ulong userId, string content)
    {
        Type = messageType;
        UserId = userId;
        Content = content;
    }
    
    public CreateDirectMessageParams(MessageType messageType, Guid chatCode, string content)
    {
        Type = messageType;
        ChatCode = chatCode;
        Content = content;
    }
}