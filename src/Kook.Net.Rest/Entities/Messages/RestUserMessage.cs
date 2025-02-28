using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using Kook.API;
using Kook.Net.Converters;
using Model = Kook.API.Message;

namespace Kook.Rest;

/// <summary>
///     Represents a REST-based message sent by a user.
/// </summary>
[DebuggerDisplay(@"{DebuggerDisplay,nq}")]
public class RestUserMessage : RestMessage, IUserMessage
{
    private bool? _isMentioningEveryone;
    private bool? _isMentioningHere;
    private Quote _quote;
    private ImmutableArray<Attachment> _attachments = ImmutableArray.Create<Attachment>();
    private ImmutableArray<ICard> _cards = ImmutableArray.Create<ICard>();
    private ImmutableArray<IEmbed> _embeds;
    private ImmutableArray<RestPokeAction> _pokes;
    private ImmutableArray<uint> _roleMentionIds = ImmutableArray.Create<uint>();
    private ImmutableArray<RestRole> _roleMentions = ImmutableArray.Create<RestRole>();
    private ImmutableArray<RestGuildChannel> _channelMentions = ImmutableArray.Create<RestGuildChannel>();
    private ImmutableArray<ITag> _tags = ImmutableArray.Create<ITag>();
    
    /// <inheritdoc cref="IUserMessage.Quote"/>
    public Quote Quote => _quote;
    
    /// <inheritdoc cref="IMessage.IsPinned"/>
    public new bool? IsPinned { get; internal set; }
    /// <inheritdoc />
    public override IReadOnlyCollection<Attachment> Attachments => _attachments;
    /// <inheritdoc />  
    public override IReadOnlyCollection<ICard> Cards => _cards;
    /// <inheritdoc />  
    public override IReadOnlyCollection<IEmbed> Embeds => _embeds;
    /// <inheritdoc />
    public override IReadOnlyCollection<RestPokeAction> Pokes => _pokes;
    /// <inheritdoc />
    public override bool? MentionedEveryone => _isMentioningEveryone;
    /// <inheritdoc />
    public override bool? MentionedHere => _isMentioningHere;
    /// <inheritdoc />
    public override IReadOnlyCollection<uint> MentionedRoleIds => _roleMentionIds;
    /// <summary>
    ///     Gets a collection of the mentioned roles in the message.
    /// </summary>
    public IReadOnlyCollection<RestRole> MentionedRoles => _roleMentions;
    /// <summary>
    ///     Gets a collection of the mentioned channels in the message.
    /// </summary>
    public IReadOnlyCollection<RestGuildChannel> MentionedChannels => _channelMentions;
    /// <inheritdoc />
    public override IReadOnlyCollection<ITag> Tags => _tags;
    
    internal RestUserMessage(BaseKookClient kook, Guid id, MessageType messageType, IMessageChannel channel, IUser author, MessageSource source)
        : base(kook, id, messageType, channel, author, source)
    {
    }
    internal new static RestUserMessage Create(BaseKookClient kook, IMessageChannel channel, IUser author, Model model)
    {
        var entity = new RestUserMessage(kook, model.Id, model.Type, channel, author, MessageHelper.GetSource(model));
        entity.Update(model);
        return entity;
    }
    internal new static RestUserMessage Create(BaseKookClient kook, IMessageChannel channel, IUser author, DirectMessage model)
    {
        var entity = new RestUserMessage(kook, model.Id, model.Type, channel, author, MessageHelper.GetSource(model, author));
        entity.Update(model);
        return entity;
    }

    internal override void Update(Model model)
    {
        base.Update(model);
        var guildId = (Channel as IGuildChannel)?.GuildId;
        var guild = guildId != null ? (Kook as IKookClient).GetGuildAsync(guildId.Value, CacheMode.CacheOnly).GetAwaiter().GetResult() : null;
        if (model.Type == MessageType.Text)
            _tags = MessageHelper.ParseTags(model.Content, null, guild, MentionedUsers, TagMode.PlainText);
        else if (Type == MessageType.KMarkdown)
            _tags = MessageHelper.ParseTags(model.Content, null, guild, MentionedUsers, TagMode.KMarkdown);
        _isMentioningEveryone = model.MentionedAll;
        _isMentioningHere = model.MentionedHere;
        _roleMentionIds = model.MentionedRoles.ToImmutableArray();
        
        if (Channel is IGuildChannel guildChannel)
        {
            if (model.MentionInfo?.MentionedRoles is not null)
            {
                var value = model.MentionInfo.MentionedRoles;
                if (value.Length > 0)
                {
                    var newMentions = ImmutableArray.CreateBuilder<RestRole>(value.Length);
                    for (int i = 0; i < value.Length; i++)
                    {
                        var val = value[i];
                        if (val != null)
                            newMentions.Add(RestRole.Create(Kook, guildChannel.Guild, val));
                    }
                    _roleMentions = newMentions.ToImmutable();
                }
            }

            if (model.MentionInfo?.MentionedChannels is not null)
            {
                var value = model.MentionInfo.MentionedChannels;
                if (value.Length > 0)
                {
                    var newMentions = ImmutableArray.CreateBuilder<RestGuildChannel>(value.Length);
                    for (int i = 0; i < value.Length; i++)
                    {
                        var val = value[i];
                        if (val != null)
                            newMentions.Add(RestGuildChannel.Create(Kook, guildChannel.Guild, val));
                    }
                    _channelMentions = newMentions.ToImmutable();
                }
            }
        }
        
        if (model.Quote is not null)
        {
            IUser refMsgAuthor = MessageHelper.GetAuthor(Kook, null, model.Quote.Author);
            _quote = Quote.Create(model.Quote.Id, model.Quote.QuotedMessageId, model.Quote.Type, model.Quote.Content, model.Quote.CreateAt, refMsgAuthor);
        }

        if (model.Attachment is not null)
            _attachments = _attachments.Add(Attachment.Create(model.Attachment));

        if (Type == MessageType.Card)
        {
            _cards = MessageHelper.ParseCards(model.Content);
            _attachments = _attachments.AddRange(MessageHelper.ParseAttachments(_cards.OfType<Card>()));
        }
        _embeds = model.Embeds.Select(x => x.ToEntity()).ToImmutableArray();

        _pokes = Type == MessageType.Poke && model.MentionInfo?.Pokes is not null
            ? model.MentionInfo.Pokes.Select(x => RestPokeAction.Create(Kook, Author,
                model.MentionInfo.MentionedUsers.Select(y => RestUser.Create(Kook, y)), x)).ToImmutableArray()
            : ImmutableArray<RestPokeAction>.Empty;
    }
    
    internal override void Update(DirectMessage model)
    {
        base.Update(model);
        var guildId = (Channel as IGuildChannel)?.GuildId;
        var guild = guildId != null ? (Kook as IKookClient).GetGuildAsync(guildId.Value, CacheMode.CacheOnly).GetAwaiter().GetResult() : null;
        if (Type == MessageType.Text)
            _tags = MessageHelper.ParseTags(model.Content, null, guild, MentionedUsers, TagMode.PlainText);
        else if (Type == MessageType.KMarkdown)
            _tags = MessageHelper.ParseTags(model.Content, null, guild, MentionedUsers, TagMode.KMarkdown);
        
        if (model.Quote is not null)
        {
            IUser refMsgAuthor = MessageHelper.GetAuthor(Kook, null, model.Quote.Author);
            _quote = Quote.Create(model.Quote.Id, model.Quote.QuotedMessageId, model.Quote.Type, model.Quote.Content, model.Quote.CreateAt, refMsgAuthor);
        }

        if (model.Attachment is not null)
            _attachments = _attachments.Add(Attachment.Create(model.Attachment));

        if (Type == MessageType.Card)
        {
            _cards = MessageHelper.ParseCards(model.Content);
            _attachments = _attachments.AddRange(MessageHelper.ParseAttachments(_cards.OfType<Card>()));
        }
        _embeds = model.Embeds.Select(x => x.ToEntity()).ToImmutableArray();
        
        if (Type == MessageType.Poke && model.MentionInfo?.Pokes is not null)
        {
            IUser recipient = (Channel as IDMChannel)?.Recipient;
            IUser target = recipient is null
                ? null
                : recipient.Id == Author.Id
                    ? Kook.CurrentUser
                    : recipient;
            _pokes = model.MentionInfo.Pokes.Select(x => RestPokeAction.Create(Kook, Author, 
                new [] {target}, x)).ToImmutableArray();
        }
        else
            _pokes = ImmutableArray<RestPokeAction>.Empty;
    }
    
    /// <param name="startIndex">The zero-based index at which to begin the resolving for the specified value.</param>
    /// <inheritdoc cref="IUserMessage.Resolve(TagHandling,TagHandling,TagHandling,TagHandling,TagHandling)"/>
    public string Resolve(int startIndex, TagHandling userHandling = TagHandling.Name, TagHandling channelHandling = TagHandling.Name,
        TagHandling roleHandling = TagHandling.Name, TagHandling everyoneHandling = TagHandling.Ignore, TagHandling emojiHandling = TagHandling.Name)
        => MentionUtils.Resolve(this, startIndex, userHandling, channelHandling, roleHandling, everyoneHandling, emojiHandling);
    /// <inheritdoc />
    public string Resolve(TagHandling userHandling = TagHandling.Name, TagHandling channelHandling = TagHandling.Name,
        TagHandling roleHandling = TagHandling.Name, TagHandling everyoneHandling = TagHandling.Ignore, TagHandling emojiHandling = TagHandling.Name)
        => MentionUtils.Resolve(this, 0, userHandling, channelHandling, roleHandling, everyoneHandling, emojiHandling);

    private string DebuggerDisplay => $"{Author}: {Content} ({Id}{(Attachments is not null && Attachments.Any() ? $", {Attachments.Count} Attachment{(Attachments.Count == 1 ? string.Empty : "s")}" : string.Empty)})";

    #region IUserMessage
    
    /// <inheritdoc />
    public async Task ModifyAsync(Action<MessageProperties> func, RequestOptions options = null)
    {
        await MessageHelper.ModifyAsync(this, Kook, func, options).ConfigureAwait(false);
        MessageProperties properties = new()
        {
            Content = Content,
            Cards = Cards,
            Quote = Quote
        };
        func(properties);
        Content = properties.Content;
        _cards = properties.Cards?.ToImmutableArray() ?? ImmutableArray<ICard>.Empty;
        _quote = properties.Quote?.QuotedMessageId == Guid.Empty ? null : (Quote) properties.Quote;
    }
    
    /// <inheritdoc />
    bool? IMessage.IsPinned => IsPinned;
    /// <inheritdoc />
    IQuote IUserMessage.Quote => _quote;
    /// <inheritdoc />
    IReadOnlyCollection<ICard> IMessage.Cards => Cards;
    /// <inheritdoc />
    IReadOnlyCollection<IEmbed> IMessage.Embeds => Embeds;

    #endregion
}