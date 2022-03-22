using System.Collections.Immutable;
using System.Diagnostics;
using KaiHeiLa.API.Gateway;
using KaiHeiLa.Rest;
using UserModel = KaiHeiLa.API.User;
using MemberModel = KaiHeiLa.API.Rest.GuildMember;

namespace KaiHeiLa.WebSocket;

/// <summary>
///     Represents a WebSocket-based guild user.
/// </summary>
[DebuggerDisplay(@"{DebuggerDisplay,nq}")]
public class SocketGuildUser : SocketUser, IGuildUser
{
    #region SocketGuildUser

    internal override SocketGlobalUser GlobalUser { get; }
    /// <summary>
    ///     Gets the guild the user is in.
    /// </summary>
    public SocketGuild Guild { get; }
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
    public override string VIPAvatar { get => GlobalUser.VIPAvatar; internal set => GlobalUser.VIPAvatar = value; }
    /// <inheritdoc />
    public override bool? IsBanned { get => GlobalUser.IsBanned; internal set => GlobalUser.IsBanned = value; }
    /// <inheritdoc />
    public override bool? IsOnline { get => GlobalUser.IsOnline; internal set => GlobalUser.IsOnline = value; }
    /// <inheritdoc />
    public override bool? IsVIP { get => GlobalUser.IsVIP; internal set => GlobalUser.IsVIP = value; }
    
    private ImmutableArray<uint> _roleIds;
    
    internal SocketGuildUser(SocketGuild guild, SocketGlobalUser globalUser)
        : base(guild.KaiHeiLa, globalUser.Id)
    {
        Guild = guild;
        GlobalUser = globalUser;
    }
    internal static SocketGuildUser Create(SocketGuild guild, ClientState state, UserModel model)
    {
        var entity = new SocketGuildUser(guild, guild.KaiHeiLa.GetOrCreateUser(state, model));
        entity.Update(state, model);
        entity.UpdateRoles(Array.Empty<uint>());
        return entity;
    }
    internal static SocketGuildUser Create(SocketGuild guild, ClientState state, MemberModel model)
    {
        var entity = new SocketGuildUser(guild, guild.KaiHeiLa.GetOrCreateUser(state, model));
        entity.Update(state, model);
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
    
    private void UpdateRoles(uint[] roleIds)
    {
        ImmutableArray<uint>.Builder roles = ImmutableArray.CreateBuilder<uint>(roleIds.Length + 1);
        for (int i = 0; i < roleIds.Length; i++)
            roles.Add(roleIds[i]);
        _roleIds = roles.ToImmutable();
    }
    
    /// <inheritdoc />
    public Task ModifyNicknameAsync(Action<string> func, RequestOptions options = null)
        => UserHelper.ModifyNicknameAsync(this, KaiHeiLa, func, options);
    /// <inheritdoc />
    public Task KickAsync(RequestOptions options = null)
        => UserHelper.KickAsync(this, KaiHeiLa, options);
    /// <inheritdoc />
    public Task AddRoleAsync(uint roleId, RequestOptions options = null)
        => AddRolesAsync(new[] { roleId }, options);
    /// <inheritdoc />
    public Task AddRoleAsync(IRole role, RequestOptions options = null)
        => AddRoleAsync(role.Id, options);
    /// <inheritdoc />
    public Task AddRolesAsync(IEnumerable<uint> roleIds, RequestOptions options = null)
        => UserHelper.AddRolesAsync(this, KaiHeiLa, roleIds, options);
    /// <inheritdoc />
    public Task AddRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null)
        => AddRolesAsync(roles.Select(x => x.Id), options);
    /// <inheritdoc />
    public Task RemoveRoleAsync(uint roleId, RequestOptions options = null)
        => RemoveRolesAsync(new[] { roleId }, options);
    /// <inheritdoc />
    public Task RemoveRoleAsync(IRole role, RequestOptions options = null)
        => RemoveRoleAsync(role.Id, options);
    /// <inheritdoc />
    public Task RemoveRolesAsync(IEnumerable<uint> roleIds, RequestOptions options = null)
        => UserHelper.RemoveRolesAsync(this, KaiHeiLa, roleIds, options);
    /// <inheritdoc />
    public Task RemoveRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null)
        => RemoveRolesAsync(roles.Select(x => x.Id));
    /// <inheritdoc />
    public Task MuteAsync(RequestOptions options = null) 
        => GuildHelper.MuteUserAsync(this, KaiHeiLa, options);
    /// <inheritdoc />
    public Task DeafenAsync(RequestOptions options = null) 
        => GuildHelper.DeafenUserAsync(this, KaiHeiLa, options);
    /// <inheritdoc />
    public Task UnmuteAsync(RequestOptions options = null) 
        => GuildHelper.UnmuteUserAsync(this, KaiHeiLa, options);
    /// <inheritdoc />
    public Task UndeafenAsync(RequestOptions options = null) 
        => GuildHelper.UndeafenUserAsync(this, KaiHeiLa, options);
    /// <inheritdoc />
    public Task<IReadOnlyCollection<IVoiceChannel>> GetConnectedVoiceChannelAsync(RequestOptions options = null)
        => SocketUserHelper.GetConnectedChannelAsync(this, KaiHeiLa, options);

    /// <inheritdoc />
    public GuildPermissions GuildPermissions => new GuildPermissions(Permissions.ResolveGuild(Guild, this));
    
    /// <summary>
    ///     Returns a collection of roles that the user possesses.
    /// </summary>
    public IReadOnlyCollection<SocketRole> Roles
        => _roleIds.Select(id => Guild.GetRole(id)).Where(x => x != null).ToReadOnlyCollection(() => _roleIds.Length);
    
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

    #endregion
    
    private string DebuggerDisplay => $"{Username}#{IdentifyNumber} ({Id}{(IsBot ?? false ? ", Bot" : "")}, Guild)";
    internal new SocketGuildUser Clone() => MemberwiseClone() as SocketGuildUser;
}