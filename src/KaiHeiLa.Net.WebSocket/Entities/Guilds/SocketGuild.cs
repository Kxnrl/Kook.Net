using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using KaiHeiLa.API;
using KaiHeiLa.API.Rest;
using Model = KaiHeiLa.API.Guild;
using ChannelModel = KaiHeiLa.API.Channel;
using MemberModel = KaiHeiLa.API.Rest.GuildMember;
using ExtendedModel = KaiHeiLa.API.Rest.ExtendedGuild;
using RoleModel = KaiHeiLa.API.Role;
using UserModel = KaiHeiLa.API.User;

namespace KaiHeiLa.WebSocket;

/// <summary>
///     Represents a WebSocket-based guild object.
/// </summary>
[DebuggerDisplay(@"{DebuggerDisplay,nq}")]
public class SocketGuild : SocketEntity<ulong>, IGuild, IDisposable
{
    #region SocketGuild

    private ConcurrentDictionary<ulong, SocketGuildChannel> _channels;
    private ConcurrentDictionary<ulong, SocketGuildUser> _members;
    private ConcurrentDictionary<uint, SocketRole> _roles;
    
    public string Name { get; private set; }
    public string Topic { get; private set; }
    public uint MasterId { get; private set; }
    public string Icon { get; private set; }
    public NotifyType NotifyType { get; private set; }
    public string Region { get; private set; }
    public bool IsOpenEnabled { get; private set; }
    public uint OpenId { get; private set; }
    public ulong DefaultChannelId { get; private set; }
    public ulong WelcomeChannelId { get; private set; }
    
    
    public object[] Features { get; private set; }

    public int BoostNumber { get; private set; }
    
    public int BufferBoostNumber { get; private set; }

    public BoostLevel BoostLevel { get; private set; }
    
    public int Status { get; private set; }

    public string AutoDeleteTime { get; private set; }

    // TODO: Public RecommendInfo
    internal RecommendInfo RecommendInfo { get; private set; }
    
    /// <summary>
    ///     Gets the number of members.
    /// </summary>
    /// <remarks>
    ///     This property retrieves the number of members returned by KaiHeiLa.
    ///     <note type="tip">
    ///     <para>
    ///         Due to how this property is returned by KaiHeiLa instead of relying on the WebSocket cache, the
    ///         number here is the most accurate in terms of counting the number of users within this guild.
    ///     </para>
    ///     <para>
    ///         Use this instead of enumerating the count of the
    ///         <see cref="KaiHeiLa.WebSocket.SocketGuild.Users" /> collection, as you may see discrepancy
    ///         between that and this property.
    ///     </para>
    ///     </note>
    /// </remarks>
    public int MemberCount { get; internal set; }
    /// <summary> Gets the number of members downloaded to the local guild cache. </summary>
    public int DownloadedMemberCount { get; private set; }
    
    internal bool IsAvailable { get; private set; }
    /// <summary> Indicates whether the client is connected to this guild. </summary>
    public bool IsConnected { get; internal set; }
    /// <summary> Indicates whether the client has all the members downloaded to the local guild cache. </summary>
    public bool HasAllMembers => MemberCount <= DownloadedMemberCount;
    
    /// <summary>
    ///     Gets the current logged-in user.
    /// </summary>
    public SocketGuildUser CurrentUser => _members.TryGetValue(KaiHeiLa.CurrentUser.Id, out SocketGuildUser member) ? member : null;
    /// <summary>
    ///     Gets the built-in role containing all users in this guild.
    /// </summary>
    /// <returns>
    ///     A role object that represents an <c>@everyone</c> role in this guild.
    /// </returns>
    public SocketRole EveryoneRole => GetRole(0);
    /// <summary>
    ///     Gets a collection of all channels in this guild.
    /// </summary>
    /// <returns>
    ///     A read-only collection of generic channels found within this guild.
    /// </returns>
    public IReadOnlyCollection<SocketGuildChannel> Channels
    {
        get
        {
            var channels = _channels;
            var state = KaiHeiLa.State;
            return channels.Select(x => x.Value).Where(x => x != null).ToReadOnlyCollection(channels);
        }
    }
    /// <summary>
    ///     Gets a collection of users in this guild.
    /// </summary>
    /// <remarks>
    ///     This property retrieves all users found within this guild.
    ///     <note type="warning">
    ///         <para>
    ///             This property may not always return all the members for large guilds (i.e. guilds containing
    ///             100+ users). If you are simply looking to get the number of users present in this guild,
    ///             consider using <see cref="MemberCount"/> instead.
    ///         </para>
    ///         <para>
    ///             Otherwise, you may need to enable <see cref="KaiHeiLaSocketConfig.AlwaysDownloadUsers"/> to fetch
    ///             the full user list upon startup, or use <see cref="DownloadUsersAsync"/> to manually download
    ///             the users.
    ///         </para>
    ///     </note>
    /// </remarks>
    /// <returns>
    ///     A collection of guild users found within this guild.
    /// </returns>
    public IReadOnlyCollection<SocketGuildUser> Users => _members.ToReadOnlyCollection();
    /// <summary>
    ///     Gets a collection of all roles in this guild.
    /// </summary>
    /// <returns>
    ///     A read-only collection of roles found within this guild.
    /// </returns>
    public IReadOnlyCollection<SocketRole> Roles => _roles.ToReadOnlyCollection();
    
    
    internal SocketGuild(KaiHeiLaSocketClient kaiHeiLa, ulong id) : base(kaiHeiLa, id)
    {
    }
    internal static SocketGuild Create(KaiHeiLaSocketClient client, ClientState state, Model model)
    {
        var entity = new SocketGuild(client, model.Id);
        entity.Update(state, model);
        return entity;
    }

    internal void Update(ClientState state, IReadOnlyCollection<ChannelModel> models)
    {
        var channels = new ConcurrentDictionary<ulong, SocketGuildChannel>(ConcurrentHashSet.DefaultConcurrencyLevel,
            (int) (models.Count * 1.05));
        foreach (ChannelModel model in models)
        {
            var channel = SocketGuildChannel.Create(this, state, model);
            state.AddChannel(channel);
            channels.TryAdd(channel.Id, channel);
        }

        _channels = channels;
    }
    internal void Update(ClientState state, IReadOnlyCollection<MemberModel> models)
    {
        var members = new ConcurrentDictionary<ulong, SocketGuildUser>(ConcurrentHashSet.DefaultConcurrencyLevel, (int)(models.Count * 1.05));
        foreach (MemberModel model in models)
        {
            var member = SocketGuildUser.Create(this, state, model);
            if (members.TryAdd(member.Id, member))
                member.GlobalUser.AddRef();
        }
        
        DownloadedMemberCount = members.Count;
        _members = members;
        MemberCount = members.Count;
    }
    internal void Update(ClientState state, ExtendedModel model)
    {
        Update(state, model as Model);

        Features = model.Features;
        BoostNumber = model.BoostNumber;
        BufferBoostNumber = model.BufferBoostNumber;
        BoostLevel = model.BoostLevel;
        Status = model.Status;
        AutoDeleteTime = model.AutoDeleteTime;
        RecommendInfo = model.RecommendInfo;
    }
    
    internal void Update(ClientState state, Model model)
    {
        Name = model.Name;
        Topic = model.Topic;
        MasterId = model.MasterId;
        Icon = model.Icon;
        NotifyType = model.NotifyType;
        Region = model.Region;
        IsOpenEnabled = model.EnableOpen;
        OpenId = model.OpenId;
        DefaultChannelId = model.DefaultChannelId;
        WelcomeChannelId = model.WelcomeChannelId;

        IsAvailable = true;

        var roles = new ConcurrentDictionary<uint, SocketRole>(ConcurrentHashSet.DefaultConcurrencyLevel,
            (int) ((model.Roles ?? Array.Empty<Role>()).Length * 1.05));
        if (model.Roles != null)
        {
            for (int i = 0; i < (model.Roles ?? Array.Empty<Role>()).Length; i++)
            {
                var role = SocketRole.Create(this, state, model.Roles![i]);
                roles.TryAdd(role.Id, role);
            }
        }
        _roles = roles;

        var channels = new ConcurrentDictionary<ulong, SocketGuildChannel>(ConcurrentHashSet.DefaultConcurrencyLevel,
            (int) ((model.Channels ?? Array.Empty<Channel>()).Length * 1.05));
        {
            for (int i = 0; i < (model.Channels ?? Array.Empty<Channel>()).Length; i++)
            {
                var channel = SocketGuildChannel.Create(this, state, model.Channels![i]);
                state.AddChannel(channel);
                channels.TryAdd(channel.Id, channel);
            }
        }
        _channels = channels;

        _members ??= new ConcurrentDictionary<ulong, SocketGuildUser>();
    }
    #endregion
    
    /// <summary>
    ///     Gets the name of the guild.
    /// </summary>
    /// <returns>
    ///     A string that resolves to <see cref="KaiHeiLa.WebSocket.SocketGuild.Name"/>.
    /// </returns>
    public override string ToString() => Name;
    private string DebuggerDisplay => $"{Name} ({Id})";

    #region IGuild

    /// <inheritdoc />
    bool IGuild.Available => true;
    
    public void Dispose()
    {
    }
    
    /// <inheritdoc />
    Task<IGuildUser> IGuild.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
        => Task.FromResult<IGuildUser>(GetUser(id));

    /// <inheritdoc />
    Task<IGuildChannel> IGuild.GetChannelAsync(ulong id, CacheMode mode, RequestOptions options)
        => Task.FromResult<IGuildChannel>(GetChannel(id));
    #endregion

    #region Channels

    /// <summary>
    ///     Gets a channel in this guild.
    /// </summary>
    /// <param name="id">The identifier for the channel.</param>
    /// <returns>
    ///     A generic channel associated with the specified <paramref name="id" />; <see langword="null"/> if none is found.
    /// </returns>
    public SocketGuildChannel GetChannel(ulong id)
    {
        var channel = KaiHeiLa.State.GetChannel(id) as SocketGuildChannel;
        if (channel?.Guild.Id == Id)
            return channel;
        return null;
    }
    /// <summary>
    ///     Gets a text channel in this guild.
    /// </summary>
    /// <param name="id">The identifier for the text channel.</param>
    /// <returns>
    ///     A text channel associated with the specified <paramref name="id" />; <see langword="null"/> if none is found.
    /// </returns>
    public SocketTextChannel GetTextChannel(ulong id)
        => GetChannel(id) as SocketTextChannel;
    
    internal SocketGuildChannel AddChannel(ClientState state, ChannelModel model)
    {
        var channel = SocketGuildChannel.Create(this, state, model);
        _channels.TryAdd(model.Id, channel);
        state.AddChannel(channel);
        return channel;
    }

    internal SocketGuildChannel AddOrUpdateChannel(ClientState state, ChannelModel model)
    {
        if (_channels.TryGetValue(model.Id, out SocketGuildChannel channel))
            channel.Update(KaiHeiLa.State, model);
        else
        {
            channel = SocketGuildChannel.Create(this, KaiHeiLa.State, model);
            _channels[channel.Id] = channel;
            state.AddChannel(channel);
        }
        return channel;
    }

    internal SocketGuildChannel RemoveChannel(ClientState state, ulong id)
    {
        if (_channels.TryRemove(id, out var _))
            return state.RemoveChannel(id) as SocketGuildChannel;
        return null;
    }
    internal void PurgeChannelCache(ClientState state)
    {
        foreach (var channelId in _channels)
            state.RemoveChannel(channelId.Key);

        _channels.Clear();
    }

    #endregion
    
    #region Roles

    /// <summary>
    ///     Gets a role in this guild.
    /// </summary>
    /// <param name="id">The identifier for the role.</param>
    /// <returns>
    ///     A role that is associated with the specified <paramref name="id"/>; <see langword="null"/> if none is found.
    /// </returns>
    public SocketRole GetRole(uint id)
    {
        if (_roles.TryGetValue(id, out SocketRole value))
            return value;
        return null;
    }

    #endregion
    
    #region Users

    /// <summary>
    ///     Gets a user from this guild.
    /// </summary>
    /// <remarks>
    ///     This method retrieves a user found within this guild.
    ///     <note>
    ///         This may return <see langword="null"/> in the WebSocket implementation due to incomplete user collection in
    ///         large guilds.
    ///     </note>
    /// </remarks>
    /// <param name="id">The identifier of the user.</param>
    /// <returns>
    ///     A guild user associated with the specified <paramref name="id"/>; <see langword="null"/> if none is found.
    /// </returns>
    public SocketGuildUser GetUser(ulong id)
    {
        if (_members.TryGetValue(id, out SocketGuildUser member))
            return member;
        return null;
    }

    internal SocketGuildUser AddOrUpdateUser(UserModel model)
    {
        if (_members.TryGetValue(model.Id, out SocketGuildUser member))
            member.GlobalUser?.Update(KaiHeiLa.State, model);
        else
        {
            member = SocketGuildUser.Create(this, KaiHeiLa.State, model);
            member.GlobalUser.AddRef();
            _members[member.Id] = member;
            DownloadedMemberCount++;
        }
        return member;
    }
    internal SocketGuildUser AddOrUpdateUser(MemberModel model)
    {
        if (_members.TryGetValue(model.Id, out SocketGuildUser member))
            member.Update(KaiHeiLa.State, model);
        else
        {
            member = SocketGuildUser.Create(this, KaiHeiLa.State, model);
            member.GlobalUser.AddRef();
            _members[member.Id] = member;
            DownloadedMemberCount++;
        }
        return member;
    }
    internal SocketGuildUser RemoveUser(ulong id)
    {
        if (_members.TryRemove(id, out SocketGuildUser member))
        {
            DownloadedMemberCount--;
            member.GlobalUser.RemoveRef(KaiHeiLa);
            return member;
        }
        return null;
    }

    /// <summary>
    ///     Purges this guild's user cache.
    /// </summary>
    public void PurgeUserCache() => PurgeUserCache(_ => true);
    /// <summary>
    ///     Purges this guild's user cache.
    /// </summary>
    /// <param name="predicate">The predicate used to select which users to clear.</param>
    public void PurgeUserCache(Func<SocketGuildUser, bool> predicate)
    {
        var membersToPurge = Users.Where(x => predicate.Invoke(x) && x?.Id != KaiHeiLa.CurrentUser.Id);
        var membersToKeep = Users.Where(x => !predicate.Invoke(x) || x?.Id == KaiHeiLa.CurrentUser.Id);

        foreach (var member in membersToPurge)
            if(_members.TryRemove(member.Id, out _))
                member.GlobalUser.RemoveRef(KaiHeiLa);

        foreach (var member in membersToKeep)
            _members.TryAdd(member.Id, member);

        DownloadedMemberCount = _members.Count;
    }

    // /// <summary>
    // ///     Gets a collection of all users in this guild.
    // /// </summary>
    // /// <remarks>
    // ///     <para>This method retrieves all users found within this guild through REST.</para>
    // ///     <para>Users returned by this method are not cached.</para>
    // /// </remarks>
    // /// <param name="options">The options to be used when sending the request.</param>
    // /// <returns>
    // ///     A task that represents the asynchronous get operation. The task result contains a collection of guild
    // ///     users found within this guild.
    // /// </returns>
    // public IAsyncEnumerable<IReadOnlyCollection<IGuildUser>> GetUsersAsync(RequestOptions options = null)
    // {
    //     if (HasAllMembers)
    //         return ImmutableArray.Create(Users).ToAsyncEnumerable<IReadOnlyCollection<IGuildUser>>();
    //     return GuildHelper.GetUsersAsync(this, KaiHeiLa, null, null, options);
    // }
    public async Task DownloadUsersAsync()
    {
        await KaiHeiLa.DownloadUsersAsync(new[] { this }).ConfigureAwait(false);
    }
    
    #endregion

    #region IGuild
    
    /// <inheritdoc />
    IRole IGuild.GetRole(uint id)
        => GetRole(id);

    /// <inheritdoc />
    IRole IGuild.EveryoneRole => EveryoneRole;
    
    #endregion
}