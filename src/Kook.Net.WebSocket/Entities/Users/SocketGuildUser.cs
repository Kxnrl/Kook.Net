using System.Collections.Immutable;
using System.Diagnostics;
using Kook.API.Gateway;
using Kook.Rest;
using UserModel = Kook.API.User;
using MemberModel = Kook.API.Rest.GuildMember;

namespace Kook.WebSocket;

/// <summary>
///     Represents a WebSocket-based guild user.
/// </summary>
[DebuggerDisplay(@"{DebuggerDisplay,nq}")]
public class SocketGuildUser : SocketUser, IGuildUser, IUpdateable
{
    #region SocketGuildUser
    
    private ImmutableArray<uint> _roleIds;
    
    internal override SocketGlobalUser GlobalUser { get; }
    /// <summary>
    ///     Gets the guild the user is in.
    /// </summary>
    public SocketGuild Guild { get; }
    /// <inheritdoc />
    public string DisplayName => Nickname ?? Username;
    /// <inheritdoc />
    public string Nickname { get; private set; }
    /// <inheritdoc />
    public bool IsMobileVerified { get; private set; }
    
    /// <inheritdoc />
    public DateTimeOffset JoinedAt { get; private set; }
    /// <inheritdoc />
    public DateTimeOffset ActiveAt { get; private set; }
    /// <inheritdoc />
    public Color Color { get; private set; }
    /// <inheritdoc />
    public bool? IsOwner { get; private set; }
    
    /// <inheritdoc />
    public new string PlainTextMention => MentionUtils.PlainTextMentionUser(Nickname ?? Username, Id);
    
    /// <inheritdoc />
    public override bool? IsBot { get => GlobalUser.IsBot; internal set => GlobalUser.IsBot = value; }
    /// <inheritdoc />
    public override string Username { get => GlobalUser.Username; internal set => GlobalUser.Username = value; }
    /// <inheritdoc />
    public override ushort? IdentifyNumberValue { get => GlobalUser.IdentifyNumberValue; internal set => GlobalUser.IdentifyNumberValue = value; }
    /// <inheritdoc />
    public override string Avatar { get => GlobalUser.Avatar; internal set => GlobalUser.Avatar = value; }
    /// <inheritdoc />
    public override string BuffAvatar { get => GlobalUser.BuffAvatar; internal set => GlobalUser.BuffAvatar = value; }
    /// <inheritdoc />
    public override bool? IsBanned { get => GlobalUser.IsBanned; internal set => GlobalUser.IsBanned = value; }
    /// <inheritdoc />
    public override bool? HasBuff { get => GlobalUser.HasBuff; internal set => GlobalUser.HasBuff = value; }
    /// <inheritdoc />
    public override bool? IsDenoiseEnabled { get => GlobalUser.IsDenoiseEnabled; internal set => GlobalUser.IsDenoiseEnabled = value; }
    /// <inheritdoc />
    public override UserTag UserTag { get => GlobalUser.UserTag; internal set => GlobalUser.UserTag = value; }
    
    /// <inheritdoc />
    public GuildPermissions GuildPermissions => new GuildPermissions(Permissions.ResolveGuild(Guild, this));
    /// <inheritdoc />
    internal override SocketPresence Presence { get; set; }
    
    /// <inheritdoc />
    public bool? IsDeafened => VoiceState?.IsDeafened ?? false;
    /// <inheritdoc />
    public bool? IsMuted => VoiceState?.IsMuted ?? false;
    /// <summary>
    ///     Returns a collection of roles that the user possesses.
    /// </summary>
    /// <remarks>
    ///     <note type="warning">
    ///         Due to the lack of events which should be raised when a role is added or removed from a user,
    ///         this property may not be completely accurate. To ensure the most accurate results,
    ///         it is recommended to call <see cref="UpdateAsync"/> before this property is used.
    ///     </note>
    /// </remarks>
    public IReadOnlyCollection<SocketRole> Roles
        => _roleIds.Select(id => Guild.GetRole(id)).Where(x => x != null).ToReadOnlyCollection(() => _roleIds.Length);
    
    /// <summary>
    ///     Returns the voice channel the user is in, or <c>null</c> if none or unknown.
    ///     <note type="warning">
    ///         If a user connects to a voice channel before the bot has connected to the gateway,
    ///         this property will be <c>null</c> until <see cref="SocketGuild.DownloadVoiceStatesAsync"/>
    ///         or <see cref="KookSocketClient.DownloadVoiceStatesAsync"/> is called.
    ///         To ensure whether the user is in a voice channel or not, use those methods above,
    ///         or <see cref="GetConnectedVoiceChannelsAsync"/>.
    ///     </note>
    /// </summary>
    public SocketVoiceChannel VoiceChannel => VoiceState?.VoiceChannel;
    /// <summary>
    ///     Gets the voice status of the user if any.
    /// </summary>
    /// <returns>
    ///     A <see cref="SocketVoiceState" /> representing the user's voice status; <c>null</c> if the user is neither
    ///     connected to a voice channel nor is muted or deafened by the guild.
    /// </returns>
    public SocketVoiceState? VoiceState => Guild.GetVoiceState(Id);
    
    internal SocketGuildUser(SocketGuild guild, SocketGlobalUser globalUser)
        : base(guild.Kook, globalUser.Id)
    {
        Guild = guild;
        GlobalUser = globalUser;
    }
    internal static SocketGuildUser Create(SocketGuild guild, ClientState state, UserModel model)
    {
        var entity = new SocketGuildUser(guild, guild.Kook.GetOrCreateUser(state, model));
        entity.Update(state, model);
        entity.UpdatePresence(model.Online, model.OperatingSystem);
        entity.UpdateRoles(Array.Empty<uint>());
        return entity;
    }
    internal static SocketGuildUser Create(SocketGuild guild, ClientState state, MemberModel model)
    {
        var entity = new SocketGuildUser(guild, guild.Kook.GetOrCreateUser(state, model));
        entity.Update(state, model);
        entity.UpdatePresence(model.Online, model.OperatingSystem);
        return entity;
    }

    internal void Update(ClientState state, MemberModel model)
    {
        base.Update(state, model);
        Nickname = model.Nickname;
        IsMobileVerified = model.MobileVerified;
        JoinedAt = model.JoinedAt;
        ActiveAt = model.ActiveAt;
        Color = new Color(model.Color);
        IsOwner = model.IsOwner;
        UpdateRoles(model.Roles);
    }

    internal void Update(ClientState state, GuildMemberUpdateEvent model)
    {
        Nickname = model.Nickname;
    }

    internal override void UpdatePresence(bool? isOnline)
    {
        base.UpdatePresence(isOnline);
        GlobalUser.UpdatePresence(isOnline);
    }
    internal override void UpdatePresence(bool? isOnline, string activeClient)
    {
        base.UpdatePresence(isOnline, activeClient);
        GlobalUser.UpdatePresence(isOnline, activeClient);
    }
    private void UpdateRoles(uint[] roleIds)
    {
        ImmutableArray<uint>.Builder roles = ImmutableArray.CreateBuilder<uint>(roleIds.Length + 1);
        roles.Add(0);
        for (int i = 0; i < roleIds.Length; i++)
            roles.Add(roleIds[i]);
        _roleIds = roles.ToImmutable();
    }
    
    /// <inheritdoc />
    public Task ModifyNicknameAsync(Action<string> func, RequestOptions options = null)
        => UserHelper.ModifyNicknameAsync(this, Kook, func, options);
    /// <inheritdoc />
    public Task KickAsync(RequestOptions options = null)
        => UserHelper.KickAsync(this, Kook, options);
    /// <inheritdoc />
    /// <note type="warning">
    ///     Due to the lack of events which should be raised when a role is added to a user,
    ///     the <see cref="SocketGuildUser.Roles"/> property will not be updated immediately after
    ///     calling this method. To update the cached roles of this user, please use <see cref="UpdateAsync"/>.
    /// </note>
    public Task AddRoleAsync(uint roleId, RequestOptions options = null)
        => AddRolesAsync(new[] { roleId }, options);
    /// <inheritdoc />
    /// <note type="warning">
    ///     Due to the lack of events which should be raised when a role is added to a user,
    ///     the <see cref="SocketGuildUser.Roles"/> property will not be updated immediately after
    ///     calling this method. To update the cached roles of this user, please use <see cref="UpdateAsync"/>.
    /// </note>
    public Task AddRoleAsync(IRole role, RequestOptions options = null)
        => AddRoleAsync(role.Id, options);
    /// <inheritdoc />
    /// <note type="warning">
    ///     Due to the lack of events which should be raised when a role is added to a user,
    ///     the <see cref="SocketGuildUser.Roles"/> property will not be updated immediately after
    ///     calling this method. To update the cached roles of this user, please use <see cref="UpdateAsync"/>.
    /// </note>
    public Task AddRolesAsync(IEnumerable<uint> roleIds, RequestOptions options = null)
        => UserHelper.AddRolesAsync(this, Kook, roleIds, options);
    /// <inheritdoc />
    /// <note type="warning">
    ///     Due to the lack of events which should be raised when a role is added to a user,
    ///     the <see cref="SocketGuildUser.Roles"/> property will not be updated immediately after
    ///     calling this method. To update the cached roles of this user, please use <see cref="UpdateAsync"/>.
    /// </note>
    public Task AddRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null)
        => AddRolesAsync(roles.Select(x => x.Id), options);
    /// <inheritdoc />
    /// <note type="warning">
    ///     Due to the lack of events which should be raised when a role is removed from a user,
    ///     the <see cref="SocketGuildUser.Roles"/> property will not be updated immediately after
    ///     calling this method. To update the cached roles of this user, please use <see cref="UpdateAsync"/>.
    /// </note>
    public Task RemoveRoleAsync(uint roleId, RequestOptions options = null)
        => RemoveRolesAsync(new[] { roleId }, options);
    /// <inheritdoc />
    /// <note type="warning">
    ///     Due to the lack of events which should be raised when a role is removed from a user,
    ///     the <see cref="SocketGuildUser.Roles"/> property will not be updated immediately after
    ///     calling this method. To update the cached roles of this user, please use <see cref="UpdateAsync"/>.
    /// </note>
    public Task RemoveRoleAsync(IRole role, RequestOptions options = null)
        => RemoveRoleAsync(role.Id, options);
    /// <inheritdoc />
    /// <note type="warning">
    ///     Due to the lack of events which should be raised when a role is removed from a user,
    ///     the <see cref="SocketGuildUser.Roles"/> property will not be updated immediately after
    ///     calling this method. To update the cached roles of this user, please use <see cref="UpdateAsync"/>.
    /// </note>
    public Task RemoveRolesAsync(IEnumerable<uint> roleIds, RequestOptions options = null)
        => UserHelper.RemoveRolesAsync(this, Kook, roleIds, options);
    /// <inheritdoc />
    /// <note type="warning">
    ///     Due to the lack of events which should be raised when a role is removed from a user,
    ///     the <see cref="SocketGuildUser.Roles"/> property will not be updated immediately after
    ///     calling this method. To update the cached roles of this user, please use <see cref="UpdateAsync"/>.
    /// </note>
    public Task RemoveRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null)
        => RemoveRolesAsync(roles.Select(x => x.Id));
    /// <inheritdoc />
    public Task MuteAsync(RequestOptions options = null) 
        => GuildHelper.MuteUserAsync(this, Kook, options);
    /// <inheritdoc />
    public Task DeafenAsync(RequestOptions options = null) 
        => GuildHelper.DeafenUserAsync(this, Kook, options);
    /// <inheritdoc />
    public Task UnmuteAsync(RequestOptions options = null) 
        => GuildHelper.UnmuteUserAsync(this, Kook, options);
    /// <inheritdoc />
    public Task UndeafenAsync(RequestOptions options = null) 
        => GuildHelper.UndeafenUserAsync(this, Kook, options);
    /// <inheritdoc cref="IGuildUser.GetConnectedVoiceChannelsAsync"/>
    public async Task<IReadOnlyCollection<SocketVoiceChannel>> GetConnectedVoiceChannelsAsync(RequestOptions options = null)
    {
        IReadOnlyCollection<SocketVoiceChannel> channels =
            await SocketUserHelper.GetConnectedChannelsAsync(this, Kook, options).ConfigureAwait(false);
        foreach (SocketVoiceChannel channel in channels)
            channel.Guild.AddOrUpdateVoiceState(Id, channel.Id);
        return channels;
    }
    /// <summary>
    ///     Fetches the users data from the REST API to update this object,
    ///     especially the <see cref="Roles"/> property.
    /// </summary>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task that represents the asynchronous reloading operation.
    /// </returns>
    public Task UpdateAsync(RequestOptions options = null)
        => SocketUserHelper.UpdateAsync(this, Kook, options);

    /// <inheritdoc />
    public ChannelPermissions GetPermissions(IGuildChannel channel)
        => new ChannelPermissions(Permissions.ResolveChannel(Guild, this, channel, GuildPermissions.RawValue));

    #endregion

    #region IGuildUser

    /// <inheritdoc />
    IGuild IGuildUser.Guild => Guild;
    /// <inheritdoc />
    ulong IGuildUser.GuildId => Guild.Id;
    /// <inheritdoc />
    IReadOnlyCollection<uint> IGuildUser.RoleIds => _roleIds;
    /// <inheritdoc />
    async Task<IReadOnlyCollection<IVoiceChannel>> IGuildUser.GetConnectedVoiceChannelsAsync(RequestOptions options)
        => await GetConnectedVoiceChannelsAsync(options).ConfigureAwait(false);

    #endregion

    #region IVoiceState
    
    /// <inheritdoc />
    IVoiceChannel IVoiceState.VoiceChannel => VoiceChannel;

    #endregion
    
    private string DebuggerDisplay => $"{Username}#{IdentifyNumber} ({Id}{(IsBot ?? false ? ", Bot" : "")}, Guild)";
    internal new SocketGuildUser Clone() => MemberwiseClone() as SocketGuildUser;
}