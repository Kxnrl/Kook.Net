namespace Kook;

/// <summary>
///     Provides properties that are used to modify an <see cref="IVoiceChannel"/> with the specified properties.
/// </summary>
/// <seealso cref="IVoiceChannel.ModifyAsync(System.Action{ModifyVoiceChannelProperties}, RequestOptions)"/>
public class ModifyVoiceChannelProperties : ModifyGuildChannelProperties
{
    /// <summary>
    ///     Gets or sets the voice quality that the clients in this voice channel are requested to use;
    ///     <c>null</c> if not set.
    /// </summary>
    /// <remarks>
    ///     <note type="warning">
    ///         This property cannot be set to the quality equivalent or higher than
    ///         <see cref="Kook.VoiceQuality._128kbps"/> via Kook REST API
    ///         because of the server-side limitation despite of the fact that
    ///         the voice channel exists in a boosted guild. 
    ///     </note>
    /// </remarks>
    public VoiceQuality? VoiceQuality { get; set; }

    /// <summary>
    ///     Gets or sets the maximum number of users that can be present in a channel, or <c>0</c> if none;
    ///     <c>null</c> if not set.
    /// </summary>
    public int? UserLimit { get; set; }
}