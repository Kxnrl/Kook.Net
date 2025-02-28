using System.Collections.Immutable;
using System.Diagnostics;
using Kook.Rest;
using Model = Kook.API.Channel;

namespace Kook.WebSocket;

/// <summary>
///     Represents a WebSocket-based voice channel in a guild.
/// </summary>
[DebuggerDisplay(@"{DebuggerDisplay,nq}")]
public class SocketVoiceChannel : SocketGuildChannel, IVoiceChannel, ISocketAudioChannel
{
    #region SocketVoiceChannel

    /// <inheritdoc />
    public ulong? CategoryId { get; set; }
    /// <summary>
    ///     Gets the parent (category) of this channel in the guild's channel list.
    /// </summary>
    /// <returns>
    ///     An <see cref="ICategoryChannel"/> representing the parent of this channel; <c>null</c> if none is set.
    /// </returns>
    public ICategoryChannel Category
        => CategoryId.HasValue ? Guild.GetChannel(CategoryId.Value) as ICategoryChannel : null;

    /// <inheritdoc />
    public bool? IsPermissionSynced { get; private set; }
    /// <inheritdoc />
    public virtual Task SyncPermissionsAsync(RequestOptions options = null)
        => ChannelHelper.SyncPermissionsAsync(this, Kook, options);
    /// <inheritdoc />
    public string KMarkdownMention => MentionUtils.KMarkdownMentionChannel(Id);
    /// <inheritdoc />
    public string PlainTextMention => MentionUtils.PlainTextMentionChannel(Id);
    /// <inheritdoc />
    public VoiceQuality? VoiceQuality { get; set; }
    /// <inheritdoc />
    public int? UserLimit { get; set; }
    /// <inheritdoc />
    public string ServerUrl { get; set; }
    /// <inheritdoc />
    public bool HasPassword { get; set; }
    /// <inheritdoc />
    /// <seealso cref="SocketVoiceChannel.ConnectedUsers"/>
    public override IReadOnlyCollection<SocketGuildUser> Users
        => Guild.Users.Where(x => Permissions.GetValue(
            Permissions.ResolveChannel(Guild, x, this, Permissions.ResolveGuild(Guild, x)),
            ChannelPermission.ViewChannel)).ToImmutableArray();
    /// <summary>
    ///     Gets a collection of users that are currently connected to this voice channel.
    /// </summary>
    /// <remarks>
    ///     <note type="warning">
    ///         This property may not always return all the members that are connected to this voice channel,
    ///         because uses may connected this voice channel before the bot has connected to the gateway.
    ///         To ensure accuracy, you may need to enable <see cref="KookSocketConfig.AlwaysDownloadVoiceStates"/>
    ///         to fetch the full voice states upon startup, or use <see cref="SocketGuild.DownloadVoiceStatesAsync"/>
    ///         on the guild this voice channel belongs to to manually download the users voice states,
    ///         or use <see cref="GetConnectedUsersAsync"/> to fetch the connected users from the API.
    ///     </note>
    /// </remarks>
    /// <returns>
    ///     A read-only collection of users that are currently connected to this voice channel.
    /// </returns>
    public IReadOnlyCollection<SocketGuildUser> ConnectedUsers
        => Guild.Users.Where(x => x.VoiceChannel?.Id == Id).ToImmutableArray();
    
    internal SocketVoiceChannel(KookSocketClient kook, ulong id, SocketGuild guild) 
        : base(kook, id, guild)
    {
        Type = ChannelType.Voice;
    }
    internal new static SocketVoiceChannel Create(SocketGuild guild, ClientState state, Model model)
    {
        var entity = new SocketVoiceChannel(guild.Kook, model.Id, guild);
        entity.Update(state, model);
        return entity;
    }
    /// <inheritdoc />
    internal override void Update(ClientState state, Model model)
    {
        base.Update(state, model);
        CategoryId = model.CategoryId != 0 ? model.CategoryId : null;
        VoiceQuality = model.VoiceQuality;
        UserLimit = model.UserLimit ?? 0;
        ServerUrl = model.ServerUrl;
        IsPermissionSynced = model.PermissionSync;
        HasPassword = model.HasPassword;
    }
    
    /// <inheritdoc />
    public override SocketGuildUser GetUser(ulong id)
    {
        var user = Guild.GetUser(id);
        if (user?.VoiceChannel?.Id == Id)
            return user;
        return null;
    }

    /// <inheritdoc />
    public Task ModifyAsync(Action<ModifyVoiceChannelProperties> func, RequestOptions options = null)
        => ChannelHelper.ModifyAsync(this, Kook, func, options);

    public async Task<IReadOnlyCollection<SocketGuildUser>> GetConnectedUsersAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
    {
        if (mode is CacheMode.AllowDownload)
            return await SocketChannelHelper.GetConnectedUsersAsync(this, Guild, Kook, options).ConfigureAwait(false);
        else
            return ConnectedUsers;
    }
    
    #endregion
    
    #region Invites

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<IInvite>> GetInvitesAsync(RequestOptions options = null)
        => await ChannelHelper.GetInvitesAsync(this, Kook, options).ConfigureAwait(false);
    /// <inheritdoc />
    public async Task<IInvite> CreateInviteAsync(int? maxAge = 604800, int? maxUses = null, RequestOptions options = null)
        => await ChannelHelper.CreateInviteAsync(this, Kook, maxAge, maxUses, options).ConfigureAwait(false);
    /// <inheritdoc />
    public async Task<IInvite> CreateInviteAsync(InviteMaxAge maxAge = InviteMaxAge._604800, InviteMaxUses maxUses = InviteMaxUses.Unlimited, RequestOptions options = null)
        => await ChannelHelper.CreateInviteAsync(this, Kook, maxAge, maxUses, options).ConfigureAwait(false);

    #endregion

    #region IVoiceChannel
    
    /// <inheritdoc />
    async Task<IReadOnlyCollection<IUser>> IVoiceChannel.GetConnectedUsersAsync(CacheMode mode, RequestOptions options)
        => await GetConnectedUsersAsync(mode, options).ConfigureAwait(false);

    #endregion
    
    private string DebuggerDisplay => $"{Name} ({Id}, Voice)";
    internal new SocketVoiceChannel Clone() => MemberwiseClone() as SocketVoiceChannel;
    
    #region IGuildChannel
    
    /// <inheritdoc />
    Task<IGuildUser> IGuildChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
        => Task.FromResult<IGuildUser>(GetUser(id));
    /// <inheritdoc />
    /// <seealso cref="IVoiceChannel.GetConnectedUsersAsync"/>
    IAsyncEnumerable<IReadOnlyCollection<IGuildUser>> IGuildChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
    {
        return mode == CacheMode.AllowDownload
            ? ChannelHelper.GetUsersAsync(this, Guild, Kook, KookConfig.MaxUsersPerBatch, 1, options)
            : ImmutableArray.Create<IReadOnlyCollection<IGuildUser>>(Users).ToAsyncEnumerable();
    }

    #endregion
    
    #region INestedChannel
    
    /// <inheritdoc />
    Task<ICategoryChannel> INestedChannel.GetCategoryAsync(CacheMode mode, RequestOptions options)
        => Task.FromResult(Category);

    #endregion
}