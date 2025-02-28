namespace Kook;

/// <summary>
///     Represents a generic Kook client.
/// </summary>
public interface IKookClient : IDisposable
{
    #region General

    /// <summary>
    ///     Gets the current state of connection.
    /// </summary>
    ConnectionState ConnectionState { get; }
    /// <summary>
    ///     Gets the currently logged-in user.
    /// </summary>
    ISelfUser CurrentUser { get; }
    /// <summary>
    ///     Gets the token type of the logged-in user.
    /// </summary>
    TokenType TokenType { get; }

    /// <summary>
    ///     Starts the connection between Kook and the client..
    /// </summary>
    /// <remarks>
    ///     This method will initialize the connection between the client and Kook.
    ///     <note type="important">
    ///         This method will immediately return after it is called, as it will initialize the connection on
    ///         another thread.
    ///     </note>
    /// </remarks>
    /// <returns>
    ///     A task that represents the asynchronous start operation.
    /// </returns>
    Task StartAsync();
    /// <summary>
    ///     Stops the connection between Kook and the client.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous stop operation.
    /// </returns>
    Task StopAsync();
    
    #endregion

    #region Channels
    
    /// <summary>
    ///     Gets a generic channel.
    /// </summary>
    /// <param name="id">The identifier of the channel (e.g. `381889909113225237`).</param>
    /// <param name="mode">The <see cref="CacheMode"/> that determines whether the object should be fetched from cache.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task that represents the asynchronous get operation. The task result contains the channel associated
    ///     with the identifier; <c>null</c> when the channel cannot be found.
    /// </returns>
    Task<IChannel> GetChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);
    /// <summary>
    ///     Gets a direct message channel.
    /// </summary>
    /// <param name="chatCode">The identifier of the channel.</param>
    /// <param name="mode">The <see cref="CacheMode"/> that determines whether the object should be fetched from cache.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task that represents the asynchronous get operation. The task result contains a read-only collection
    ///     of direct-message channels that the user currently partakes in.
    /// </returns>
    Task<IDMChannel> GetDMChannelAsync(Guid chatCode, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);
    /// <summary>
    ///     Gets a collection of direct message channels opened in this session.
    /// </summary>
    /// <remarks>
    ///     This method returns a collection of currently opened direct message channels.
    ///     <note type="warning">
    ///         This method will not return previously opened DM channels outside of the current session! If you
    ///         have just started the client, this may return an empty collection.
    ///     </note>
    /// </remarks>
    /// <param name="mode">The <see cref="CacheMode"/> that determines whether the object should be fetched from cache.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task that represents the asynchronous get operation. The task result contains a read-only collection
    ///     of direct-message channels that the user currently partakes in.
    /// </returns>
    Task<IReadOnlyCollection<IDMChannel>> GetDMChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);

    #endregion
    
    #region Guilds
    
    /// <summary>
    ///     Gets a guild.
    /// </summary>
    /// <param name="id">The guild identifier.</param>
    /// <param name="mode">The <see cref="CacheMode"/> that determines whether the object should be fetched from cache.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task that represents the asynchronous get operation. The task result contains the guild associated
    ///     with the identifier; <c>null</c> when the guild cannot be found.
    /// </returns>
    Task<IGuild> GetGuildAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);
    /// <summary>
    ///     Gets a collection of guilds that the user is currently in.
    /// </summary>
    /// <param name="mode">The <see cref="CacheMode"/> that determines whether the object should be fetched from cache.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task that represents the asynchronous get operation. The task result contains a read-only collection
    ///     of guilds that the current user is in.
    /// </returns>
    Task<IReadOnlyCollection<IGuild>> GetGuildsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);
    
    #endregion
    
    #region Users
    
    /// <summary>
    ///     Gets a user.
    /// </summary>
    /// <param name="id">The identifier of the user (e.g. `168693960628371456`).</param>
    /// <param name="mode">The <see cref="CacheMode"/> that determines whether the object should be fetched from cache.</param>
    /// <param name="options">The options to be used when sending the request.</param>
    /// <returns>
    ///     A task that represents the asynchronous get operation. The task result contains the user associated with
    ///     the identifier; <c>null</c> if the user is not found.
    /// </returns>
    Task<IUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null);
    
    #endregion

}