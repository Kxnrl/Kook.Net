using System.Collections.Immutable;
using System.Diagnostics;
using Kook.Rest;
using Model = Kook.API.Channel;

namespace Kook.WebSocket;

[DebuggerDisplay(@"{DebuggerDisplay,nq}")]
public class SocketGuildChannel : SocketChannel, IGuildChannel
{
    private ImmutableArray<RolePermissionOverwrite> _rolePermissionOverwrites;
    private ImmutableArray<UserPermissionOverwrite> _userPermissionOverwrites;

    #region SocketGuildChannel

    /// <summary>
    ///     Gets the guild associated with this channel.
    /// </summary>
    /// <returns>
    ///     A guild object that this channel belongs to.
    /// </returns>
    public SocketGuild Guild { get; }
    /// <inheritdoc />
    public string Name { get; private set; }
    /// <inheritdoc />
    public int? Position { get; private set; }
    /// <inheritdoc />
    public ChannelType Type { get; internal set; }
    /// <inheritdoc />
    public ulong CreatorId { get; private set; }
    /// <summary>
    ///     Gets the creator of this channel.
    /// </summary>
    /// <remarks>
    ///     This method will try to get the user as a member of this channel. If the user is not a member of this guild,
    ///     this method will return <c>null</c>. To get the creator under this circumstance, use
    ///     <see cref="Kook.Rest.KookRestClient.GetUserAsync(ulong,RequestOptions)"/>.
    /// </remarks>
    /// <returns>
    ///     A task that represents the asynchronous get operation. The task result contains the creator of this channel.
    /// </returns>
    public SocketGuildUser Creator => GetUser(CreatorId);
    /// <inheritdoc />
    public IReadOnlyCollection<RolePermissionOverwrite> RolePermissionOverwrites => _rolePermissionOverwrites;
    /// <inheritdoc />
    public IReadOnlyCollection<UserPermissionOverwrite> UserPermissionOverwrites => _userPermissionOverwrites;
    /// <summary>
    ///     Gets a collection of users that are able to view the channel.
    /// </summary>
    /// <returns>
    ///     A read-only collection of users that can access the channel (i.e. the users seen in the user list).
    /// </returns>
    public new virtual IReadOnlyCollection<SocketGuildUser> Users => ImmutableArray.Create<SocketGuildUser>();

    internal SocketGuildChannel(KookSocketClient kook, ulong id, SocketGuild guild)
        : base(kook, id)
    {
        Guild = guild;
    }
    internal static SocketGuildChannel Create(SocketGuild guild, ClientState state, Model model)
    {
        return model.Type switch
        {
            ChannelType.Category => SocketCategoryChannel.Create(guild, state, model),
            ChannelType.Text => SocketTextChannel.Create(guild, state, model),
            ChannelType.Voice => SocketVoiceChannel.Create(guild, state, model),
            _ => new SocketGuildChannel(guild.Kook, model.Id, guild),
        };
    }
    
    /// <inheritdoc />
    internal override void Update(ClientState state, Model model)
    {
        Name = model.Name;
        Position = model.Position;
        CreatorId = model.CreatorId;

        var rolePermissionOverwrites = model.RolePermissionOverwrites;
        var newRoleOverwrites = ImmutableArray.CreateBuilder<RolePermissionOverwrite>(rolePermissionOverwrites.Length);
        for (int i = 0; i < rolePermissionOverwrites.Length; i++)
            newRoleOverwrites.Add(rolePermissionOverwrites[i].ToEntity());
        _rolePermissionOverwrites = newRoleOverwrites.ToImmutable();
        
        var userPermissionOverwrites = model.UserPermissionOverwrites;
        var newUserOverwrites = ImmutableArray.CreateBuilder<UserPermissionOverwrite>(userPermissionOverwrites.Length);
        for (int i = 0; i < userPermissionOverwrites.Length; i++)
            newUserOverwrites.Add(userPermissionOverwrites[i].ToEntity(Kook, state));
        _userPermissionOverwrites = newUserOverwrites.ToImmutable();
    }

    /// <inheritdoc />
    public override Task UpdateAsync(RequestOptions options = null)
        => SocketChannelHelper.UpdateAsync(this, options);
    
    /// <inheritdoc />
    public Task ModifyAsync(Action<ModifyGuildChannelProperties> func, RequestOptions options = null)
        => ChannelHelper.ModifyAsync(this, Kook, func, options);
    /// <inheritdoc />
    public Task DeleteAsync(RequestOptions options = null)
        => ChannelHelper.DeleteGuildChannelAsync(this, Kook, options);
    
    /// <summary>
    ///     Gets the permission overwrite for a specific user.
    /// </summary>
    /// <param name="user">The user to get the overwrite from.</param>
    /// <returns>
    ///     An overwrite object for the targeted user; <c>null</c> if none is set.
    /// </returns>
    public virtual OverwritePermissions? GetPermissionOverwrite(IUser user)
    {
        for (int i = 0; i < _userPermissionOverwrites.Length; i++)
        {
            if (_userPermissionOverwrites[i].Target.Id == user.Id)
                return _userPermissionOverwrites[i].Permissions;
        }
        return null;
    }

    /// <summary>
    ///     Gets the permission overwrite for a specific role.
    /// </summary>
    /// <param name="role">The role to get the overwrite from.</param>
    /// <returns>
    ///     An overwrite object for the targeted role; <c>null</c> if none is set.
    /// </returns>
    public virtual OverwritePermissions? GetPermissionOverwrite(IRole role)
    {
        for (int i = 0; i < _rolePermissionOverwrites.Length; i++)
        {
            if (_rolePermissionOverwrites[i].Target == role.Id)
                return _rolePermissionOverwrites[i].Permissions;
        }
        return null;
    }

    /// <summary>
    ///     Adds or updates the permission overwrite for the given user.
    /// </summary>
    /// <param name="user">The user to add the overwrite to.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task representing the asynchronous permission operation for adding the specified permissions to the channel.
    /// </returns>
    public async Task AddPermissionOverwriteAsync(IGuildUser user, RequestOptions options = null)
    {
        await ChannelHelper.AddPermissionOverwriteAsync(this, Kook, user, options).ConfigureAwait(false);
    }

    /// <summary>
    ///     Adds or updates the permission overwrite for the given role.
    /// </summary>
    /// <param name="role">The role to add the overwrite to.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task representing the asynchronous permission operation for adding the specified permissions to the channel.
    /// </returns>
    public async Task AddPermissionOverwriteAsync(IRole role, RequestOptions options = null)
    {
        await ChannelHelper.AddPermissionOverwriteAsync(this, Kook, role, options).ConfigureAwait(false);
    }
    /// <summary>
    ///     Removes the permission overwrite for the given user, if one exists.
    /// </summary>
    /// <param name="user">The user to remove the overwrite from.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task representing the asynchronous operation for removing the specified permissions from the channel.
    /// </returns>
    public async Task RemovePermissionOverwriteAsync(IGuildUser user, RequestOptions options = null)
    {
        await ChannelHelper.RemovePermissionOverwriteAsync(this, Kook, user, options).ConfigureAwait(false);
    }
    /// <summary>
    ///     Removes the permission overwrite for the given role, if one exists.
    /// </summary>
    /// <param name="role">The role to remove the overwrite from.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task representing the asynchronous operation for removing the specified permissions from the channel.
    /// </returns>
    public async Task RemovePermissionOverwriteAsync(IRole role, RequestOptions options = null)
    {
        await ChannelHelper.RemovePermissionOverwriteAsync(this, Kook, role, options).ConfigureAwait(false);
    }
    
    /// <summary>
    ///     Updates the permission overwrite for the given user, if one exists.
    /// </summary>
    /// <param name="user">The user to modify the overwrite for.</param>
    /// <param name="func">A delegate containing the values to modify the permission overwrite with.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task representing the asynchronous operation for removing the specified permissions from the channel.
    /// </returns>
    public async Task ModifyPermissionOverwriteAsync(IGuildUser user, Func<OverwritePermissions, OverwritePermissions> func, RequestOptions options = null)
    {
        await ChannelHelper.ModifyPermissionOverwriteAsync(this, Kook, user, func, options).ConfigureAwait(false);
    }
    
    /// <summary>
    ///     Updates the permission overwrite for the given role, if one exists.
    /// </summary>
    /// <param name="role">The role to remove the overwrite for.</param>
    /// <param name="func">A delegate containing the values to modify the permission overwrite with.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task representing the asynchronous operation for removing the specified permissions from the channel.
    /// </returns>
    public async Task ModifyPermissionOverwriteAsync(IRole role, Func<OverwritePermissions, OverwritePermissions> func, RequestOptions options = null)
    {
        await ChannelHelper.ModifyPermissionOverwriteAsync(this, Kook, role, func, options).ConfigureAwait(false);
    }
    public new virtual SocketGuildUser GetUser(ulong id) => null;
    
    /// <summary>
    ///     Gets the name of the channel.
    /// </summary>
    /// <returns>
    ///     A string that resolves to <see cref="SocketGuildChannel.Name"/>.
    /// </returns>
    public override string ToString() => Name;
    private string DebuggerDisplay => $"{Name} ({Id}, Guild)";
    internal new SocketGuildChannel Clone() => MemberwiseClone() as SocketGuildChannel;
    
    #endregion
    
    #region SocketChannel
    
    /// <inheritdoc />
    internal override SocketUser GetUserInternal(ulong id) => GetUser(id);
    /// <inheritdoc />
    internal override IReadOnlyCollection<SocketUser> GetUsersInternal() => Users;
    
    #endregion
    
    #region IGuildChannel
    
    /// <inheritdoc />
    IGuild IGuildChannel.Guild => Guild;
    /// <inheritdoc />
    ulong IGuildChannel.GuildId => Guild.Id;
    
    /// <inheritdoc />
    async Task IGuildChannel.AddPermissionOverwriteAsync(IRole role, RequestOptions options)
        => await AddPermissionOverwriteAsync(role, options).ConfigureAwait(false);
    /// <inheritdoc />
    async Task IGuildChannel.AddPermissionOverwriteAsync(IGuildUser user, RequestOptions options)
        => await AddPermissionOverwriteAsync(user, options).ConfigureAwait(false);
    /// <inheritdoc />
    async Task IGuildChannel.RemovePermissionOverwriteAsync(IRole role, RequestOptions options)
        => await RemovePermissionOverwriteAsync(role, options).ConfigureAwait(false);
    /// <inheritdoc />
    async Task IGuildChannel.RemovePermissionOverwriteAsync(IGuildUser user, RequestOptions options)
        => await RemovePermissionOverwriteAsync(user, options).ConfigureAwait(false);
    /// <inheritdoc />
    async Task IGuildChannel.ModifyPermissionOverwriteAsync(IRole role, Func<OverwritePermissions, OverwritePermissions> func, RequestOptions options)
        => await ModifyPermissionOverwriteAsync(role, func, options).ConfigureAwait(false);
    /// <inheritdoc />
    async Task IGuildChannel.ModifyPermissionOverwriteAsync(IGuildUser user, Func<OverwritePermissions, OverwritePermissions> func, RequestOptions options)
        => await ModifyPermissionOverwriteAsync(user, func, options).ConfigureAwait(false);
    
    /// <inheritdoc />
    IAsyncEnumerable<IReadOnlyCollection<IGuildUser>> IGuildChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
        => ImmutableArray.Create<IReadOnlyCollection<IGuildUser>>(Users).ToAsyncEnumerable(); //Overridden in Text/Voice
    /// <inheritdoc />
    Task<IGuildUser> IGuildChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
        => Task.FromResult<IGuildUser>(GetUser(id)); //Overridden in Text/Voice
    
    /// <summary>
    ///     Gets the creator of this channel.
    /// </summary>
    /// <remarks>
    ///     This method will try to get the user as a member of this channel. If the user is not a member of this guild,
    ///     this method will return <c>null</c>. To get the creator under this circumstance, use
    ///     <see cref="Kook.Rest.KookRestClient.GetUserAsync(ulong,RequestOptions)"/>.
    /// </remarks>
    /// <returns>
    ///     A task that represents the asynchronous get operation. The task result contains the creator of this channel.
    /// </returns>
    Task<IUser> IGuildChannel.GetCreatorAsync(CacheMode mode, RequestOptions options)
        => Task.FromResult<IUser>(Creator);
    
    #endregion

    #region IChannel
    
    /// <inheritdoc />
    IAsyncEnumerable<IReadOnlyCollection<IUser>> IChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
        => ImmutableArray.Create<IReadOnlyCollection<IUser>>(Users).ToAsyncEnumerable(); //Overridden in Text/Voice
    /// <inheritdoc />
    Task<IUser> IChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
        => Task.FromResult<IUser>(GetUser(id)); //Overridden in Text/Voice
    
    #endregion
}