﻿namespace Kook;

/// <summary>
///     Represents a generic guild user.
/// </summary>
public interface IGuildUser : IUser, IVoiceState
{
    #region General

    /// <summary>
    ///     Gets the nickname for this user.
    /// </summary>
    /// <returns>
    ///     A string representing the nickname of the user; <c>null</c> if none is set.
    /// </returns>
    string Nickname { get; }
    /// <summary>
    ///     Gets the displayed name for this user.
    /// </summary>
    /// <returns>
    ///     A string representing the display name of the user; If the nickname is null, this will be the username.
    /// </returns>
    string DisplayName { get; }
    /// <summary>
    ///     Gets a collection of IDs for the roles that this user currently possesses in the guild.
    /// </summary>
    /// <remarks>
    ///     This property returns a read-only collection of the identifiers of the roles that this user possesses.
    ///     For WebSocket users, a Roles property can be found in place of this property. Due to the REST
    ///     implementation, only a collection of identifiers can be retrieved instead of the full role objects.
    /// </remarks>
    /// <returns>
    ///     A read-only collection of <see langword="uint"/>, each representing an identifier for a role that
    ///     this user possesses.
    /// </returns>
    IReadOnlyCollection<uint> RoleIds { get; }
    /// <summary>
    ///     Gets the guild for this user.
    /// </summary>
    /// <returns>
    ///     A guild object that this user belongs to.
    /// </returns>
    IGuild Guild { get; }
    /// <summary>
    ///     Gets the ID of the guild for this user.
    /// </summary>
    /// <returns>
    ///     An <see langword="ulong"/> representing the identifier of the guild that this user belongs to.
    /// </returns>
    ulong GuildId { get; }
    /// <summary>
    ///     Gets whether the mobile number has been verified for this user.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if the mobile number has been verified; <c>false</c> otherwise.
    /// </returns>
    bool IsMobileVerified { get; }
    /// <summary>
    ///     Gets when this user joined the guild.
    /// </summary>
    /// <returns>
    ///     The time of which the user has joined the guild.
    /// </returns>
    DateTimeOffset JoinedAt { get; }
    /// <summary>
    ///     Gets when this user was activated.
    /// </summary>
    /// <returns>
    ///     The time of which the user was activated.
    /// </returns>
    DateTimeOffset ActiveAt { get; }
    /// <summary>
    ///     Gets the color the user's displayed name is being displayed in.
    /// </summary>
    /// <returns>
    ///     A <see cref="Color"/> struct representing the color the user's display name is being displayed in.
    /// </returns>
    Color Color { get; }
    /// <summary>
    ///     Gets whether this user owns the current guild.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if this user owns the current guild; <c>false</c> otherwise.
    /// </returns>
    bool? IsOwner { get; }
    
    #endregion

    #region Permissions

    /// <summary>
    ///     Gets the guild-level permissions for this user.
    /// </summary>
    /// <returns>
    ///     A <see cref="Kook.GuildPermissions"/> structure for this user, representing what
    ///     permissions this user has in the guild.
    /// </returns>
    GuildPermissions GuildPermissions { get; }
    
    /// <summary>
    ///     Gets the level permissions granted to this user to a given channel.
    /// </summary>
    /// <param name="channel">The channel to get the permission from.</param>
    /// <returns>
    ///     A <see cref="Kook.ChannelPermissions"/> structure representing the permissions that a user has in the
    ///     specified channel.
    /// </returns>
    ChannelPermissions GetPermissions(IGuildChannel channel);

    #endregion

    #region Guild

    /// <summary>
    ///     Kicks this user from this guild.
    /// </summary>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task that represents the asynchronous kick operation.
    /// </returns>
    Task KickAsync(RequestOptions options = null);

    /// <summary>
    ///     Modifies this user's nickname in this guild.
    /// </summary>
    /// <remarks>
    ///     This method modifies the nickname of current guild user.
    /// </remarks>
    /// <param name="func">The delegate containing the nickname to modify the user with.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task that represents the asynchronous modification operation.
    /// </returns>
    Task ModifyNicknameAsync(Action<string> func, RequestOptions options = null);
    
    #endregion

    #region Roles
    
    /// <summary>
    ///     Adds the specified role to this user in the guild.
    /// </summary>
    /// <param name="roleId">The role to be added to the user.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task that represents the asynchronous role addition operation.
    /// </returns>
    Task AddRoleAsync(uint roleId, RequestOptions options = null);
    /// <summary>
    ///     Adds the specified role to this user in the guild.
    /// </summary>
    /// <param name="role">The role to be added to the user.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task that represents the asynchronous role addition operation.
    /// </returns>
    Task AddRoleAsync(IRole role, RequestOptions options = null);
    /// <summary>
    ///     Adds the specified <paramref name="roleIds"/> to this user in the guild.
    /// </summary>
    /// <param name="roleIds">The roles to be added to the user.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task that represents the asynchronous role addition operation.
    /// </returns>
    Task AddRolesAsync(IEnumerable<uint> roleIds, RequestOptions options = null);
    /// <summary>
    ///     Adds the specified <paramref name="roles"/> to this user in the guild.
    /// </summary>
    /// <param name="roles">The roles to be added to the user.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task that represents the asynchronous role addition operation.
    /// </returns>
    Task AddRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null);
    /// <summary>
    ///     Removes the specified <paramref name="roleId"/> from this user in the guild.
    /// </summary>
    /// <param name="roleId">The role to be removed from the user.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task that represents the asynchronous role removal operation.
    /// </returns>
    Task RemoveRoleAsync(uint roleId, RequestOptions options = null);
    /// <summary>
    ///     Removes the specified <paramref name="role"/> from this user in the guild.
    /// </summary>
    /// <param name="role">The role to be removed from the user.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task that represents the asynchronous role removal operation.
    /// </returns>
    Task RemoveRoleAsync(IRole role, RequestOptions options = null);
    /// <summary>
    ///     Removes the specified <paramref name="roleIds"/> from this user in the guild.
    /// </summary>
    /// <param name="roleIds">The roles to be removed from the user.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task that represents the asynchronous role removal operation.
    /// </returns>
    Task RemoveRolesAsync(IEnumerable<uint> roleIds, RequestOptions options = null);
    /// <summary>
    ///     Removes the specified <paramref name="roles"/> from this user in the guild.
    /// </summary>
    /// <param name="roles">The roles to be removed from the user.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task that represents the asynchronous role removal operation.
    /// </returns>
    Task RemoveRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null);
    
    #endregion
    
    #region Voice
    
    /// <summary>
    ///     Mute this user in this guild.
    /// </summary>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task that represents the asynchronous muting operation.
    /// </returns>
    Task MuteAsync(RequestOptions options = null);
    
    /// <summary>
    ///     Deafen this user in this guild.
    /// </summary>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task that represents the asynchronous deafening operation.
    /// </returns>
    Task DeafenAsync(RequestOptions options = null);
    
    /// <summary>
    ///     Unmute this user in this guild.
    /// </summary>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task that represents the asynchronous unmuting operation.
    /// </returns>
    Task UnmuteAsync(RequestOptions options = null);
    
    /// <summary>
    ///     Undeafen this user in this guild.
    /// </summary>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task that represents the asynchronous undeafening operation.
    /// </returns>
    Task UndeafenAsync(RequestOptions options = null);
    
    /// <summary>
    ///     Gets a collection of voice channels a user
    /// </summary>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task that represents the asynchronous get operation. The task result contains a collection of
    ///     voice channels the user is connected to.
    /// </returns>
    Task<IReadOnlyCollection<IVoiceChannel>> GetConnectedVoiceChannelsAsync(RequestOptions options = null);
    
    #endregion
}