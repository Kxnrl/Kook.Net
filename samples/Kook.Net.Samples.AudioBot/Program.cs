﻿// See https://aka.ms/new-console-template for more information

using Kook;
using Kook.Rest;
using Kook.WebSocket;

class Program
{
    private readonly KookSocketClient _client;
    private readonly string _token;
    private readonly ulong _guildId;
    private readonly ulong _channelId;
    public static Task Main(string[] args) => new Program().MainAsync();

    public Program()
    {
        _token = Environment.GetEnvironmentVariable("KookDebugToken", EnvironmentVariableTarget.User) 
                 ?? throw new ArgumentNullException(nameof(_token));
        _guildId = ulong.Parse(Environment.GetEnvironmentVariable("KookDebugGuild", EnvironmentVariableTarget.User) 
                               ?? throw new ArgumentNullException(nameof(_token)));
        _channelId = ulong.Parse(Environment.GetEnvironmentVariable("KookDebugChannel", EnvironmentVariableTarget.User) 
                                 ?? throw new ArgumentNullException(nameof(_token)));
        _client = new(new KookSocketConfig() {AlwaysDownloadUsers = true, MessageCacheSize = 100, LogLevel = LogSeverity.Verbose});
        
        _client.Log += ClientOnLog;
        _client.MessageReceived += ClientOnMessageReceived;
        _client.Ready += ClientOnReady;
    }

    public async Task MainAsync()
    {
        await _client.LoginAsync(TokenType.Bot, _token);
        await _client.StartAsync();
        await Task.Delay(Timeout.Infinite);
    }

    private async Task ClientOnMessageReceived(SocketMessage arg)
    {
        string argCleanContent = arg.CleanContent;
        if (arg.Author.Id == _client.CurrentUser.Id) return;
        if (arg.Author.IsBot == true) return;
        if (arg.Content != "/test") return;
        // await arg.Channel.SendTextAsync("收到了！", quote: new Quote(arg.Id));
        // await msg.UpdateAsync();
        await CardDemo(arg);
    }

    private async Task ClientOnReady()
    {
        IEnumerable<SocketGuildUser> socketGuildUsers = await _client.GetGuild(1990044438283387).Roles.ToList()[1].GetUsersAsync().FlattenAsync();
        await Task.Delay(5000);
        IEnumerable<SocketGuildUser> flattenAsync = await _client.GetGuild(1990044438283387).Roles.ToList()[1].GetUsersAsync().FlattenAsync();
    }

    private async Task ClientOnLog(LogMessage arg)
    {
        await Task.Delay(0);
        Console.WriteLine(arg.ToString());
    }
    
    private async Task ClientOnMessageButtonClicked(string value, SocketUser user, IMessage msg, SocketTextChannel channel, SocketGuild guild)
    {
        // await msg.AddReactionAsync(Emote.Parse("[:djbigfan:1990044438283387/hvBcVC4nHX03k03k]", TagMode.PlainText));
        // await msg.AddReactionAsync(Emote.Parse("(emj)djbigfan(emj)[1990044438283387/hvBcVC4nHX03k03k]", TagMode.KMarkdown));
        // await msg.RemoveReactionAsync(Emote.Parse("[:djbigfan:1990044438283387/hvBcVC4nHX03k03k]", TagMode.PlainText), _client.CurrentUser);
        // IEnumerable<IMessage> selectMany = (await _client.GetGuild(1990044438283387).GetTextChannel(6286033651700207).GetMessagesAsync(Guid.Parse("ed260ee9-1616-44ec-abff-d5cfcf9903a0"), Direction.Around, 5).ToListAsync()).SelectMany(x => x.ToList());
        // await _client.GetUserAsync(0);
        // IReadOnlyCollection<RestMessage> pinnedMessagesAsync = await _client.GetGuild(1990044438283387).GetTextChannel(6286033651700207).GetPinnedMessagesAsync();
        // await (user as SocketGuildUser).AddRoleAsync(1681537);
        // IEnumerable<IGuildUser> flattenAsync = await _client.GetGuild(1990044438283387).GetRole(300643).GetUsersAsync().FlattenAsync().ConfigureAwait(false);
        // IReadOnlyCollection<IInvite> readOnlyCollection = await _client.GetGuild(1990044438283387).GetInvitesAsync();
        // IInvite invite = await _client.GetGuild(1990044438283387).CreateInviteAsync(InviteMaxAge.NeverExpires, InviteMaxUses.Fifty);
        // SocketGuild socketGuild = _client.GetGuild(1990044438283387);
        // IEnumerable<RestGame> games = await _client.Rest.GetGamesAsync().FlattenAsync().ConfigureAwait(false);
        await _client.GetGuild(1990044438283387).GetTextChannel(6286033651700207)
            .SendFileAsync("E:\\OneDrive\\Pictures\\86840227_p0_新年.png", type: AttachmentType.Image);
    }

    private async Task CardDemo(SocketMessage message)
    {
        if (message.Author.Id == _client.CurrentUser.Id) return;
        if (message.Content != "/test") return;
        CardBuilder cardBuilder = new CardBuilder()
            .WithSize(CardSize.Large)
            .AddModule(new HeaderModuleBuilder().WithText("This is header"))
            .AddModule(new DividerModuleBuilder())
            .AddModule(new SectionModuleBuilder().WithText("**This** *is* ~~kmarkdown~~", true))
            .AddModule(new SectionModuleBuilder()
                .WithText(new ParagraphStructBuilder()
                    .WithColumnCount(2)
                    .AddField(new PlainTextElementBuilder().WithContent("多列文本测试"))
                    .AddField(new KMarkdownElementBuilder().WithContent("**昵称**\n白给Doc"))
                    .AddField(new KMarkdownElementBuilder().WithContent("**在线时间**\n9:00-21:00"))
                    .AddField(new KMarkdownElementBuilder().WithContent("**服务器**\n吐槽中心")))
                .WithAccessory(new ImageElementBuilder()
                    .WithSource("https://img.kaiheila.cn/assets/2021-01/7kr4FkWpLV0ku0ku.jpeg")
                    .WithSize(ImageSize.Small))
                .WithMode(SectionAccessoryMode.Unspecified))
            .AddModule(new SectionModuleBuilder()
                .WithText(new PlainTextElementBuilder().WithContent("您是否认为\"KOOK\"是最好的语音软件？"))
                .WithAccessory(new ButtonElementBuilder().WithTheme(ButtonTheme.Primary).WithText("完全同意", false))
                .WithMode(SectionAccessoryMode.Right))
            .AddModule(new ContainerModuleBuilder()
                .AddElement(new ImageElementBuilder().WithSource("https://img.kaiheila.cn/assets/2021-01/7kr4FkWpLV0ku0ku.jpeg")))
            .AddModule(new ImageGroupModuleBuilder()
                .AddElement(new ImageElementBuilder().WithSource("https://img.kaiheila.cn/assets/2021-01/pWsmcLsPJq08c08c.jpeg"))
                .AddElement(new ImageElementBuilder().WithSource("https://img.kaiheila.cn/assets/2021-01/YIfHfnvxaV0dw0dw.jpg")))
            .AddModule(new ContextModuleBuilder()
                .AddElement(new PlainTextElementBuilder().WithContent("KOOK 气氛组，等你一起来搞事情"))
                .AddElement(new ImageElementBuilder().WithSource("https://img.kaiheila.cn/assets/2021-01/7kr4FkWpLV0ku0ku.jpeg"))
                .AddElement(new ImageElementBuilder().WithSource("https://img.kaiheila.cn/assets/2021-01/7kr4FkWpLV0ku0ku.jpeg"))
                .AddElement(new ImageElementBuilder().WithSource("https://img.kaiheila.cn/assets/2021-01/7kr4FkWpLV0ku0ku.jpeg")))
            .AddModule(new ActionGroupModuleBuilder()
                .AddElement(new ButtonElementBuilder().WithTheme(ButtonTheme.Primary).WithText("确定").WithValue("ok"))
                .AddElement(new ButtonElementBuilder().WithTheme(ButtonTheme.Danger).WithText("取消").WithValue("cancel")))
            .AddModule(new FileModuleBuilder()
                .WithSource("https://img.kaiheila.cn/attachments/2021-01/21/600972b5d0d31.txt")
                .WithTitle("KOOK 介绍.txt"))
            .AddModule(new AudioModuleBuilder()
                .WithSource("https://img.kaiheila.cn/attachments/2021-01/21/600975671b9ab.mp3")
                .WithTitle("命运交响曲")
                .WithCover("https://img.kaiheila.cn/assets/2021-01/rcdqa8fAOO0hs0mc.jpg"))
            .AddModule(new VideoModuleBuilder()
                .WithSource("https://img.kaiheila.cn/attachments/2021-01/20/6008127e8c8de.mp4")
                .WithTitle("有本事别笑"))
            .AddModule(new DividerModuleBuilder())
            .AddModule(new CountdownModuleBuilder().WithMode(CountdownMode.Day).WithEndTime(DateTimeOffset.Now.AddMinutes(1)))
            .AddModule(new CountdownModuleBuilder().WithMode(CountdownMode.Hour).WithEndTime(DateTimeOffset.Now.AddMinutes(1)))
            .AddModule(new CountdownModuleBuilder().WithMode(CountdownMode.Second).WithEndTime(DateTimeOffset.Now.AddMinutes(2)).WithStartTime(DateTimeOffset.Now.AddMinutes(1)));
        
        var response = await _client.GetGuild(((SocketUserMessage) message).Guild.Id)
            .GetTextChannel(message.Channel.Id)
            .SendCardAsync(cardBuilder.Build(), quote: new Quote(message.Id));
    }

    private async Task ModifyMessageDemo()
    {
        await Task.Delay(TimeSpan.FromSeconds(1));
        
        SocketTextChannel channel = _client.GetGuild(_guildId).GetTextChannel(_channelId);
        var response = await channel
            .SendTextAsync("BeforeModification");
        await Task.Delay(TimeSpan.FromSeconds(1));
        
        IUserMessage msg = await channel.GetMessageAsync(response.Id) as IUserMessage;
        await msg!.ModifyAsync(properties => properties.Content += "\n==========\nModified");
        await Task.Delay(TimeSpan.FromSeconds(1));
        
        await msg.DeleteAsync();
        await Task.Delay(TimeSpan.FromSeconds(1));

        response = await channel.SendCardAsync(new CardBuilder()
            .AddModule(new HeaderModuleBuilder().WithText("Test")).Build());
        await Task.Delay(TimeSpan.FromSeconds(1));
        
        msg = await channel.GetMessageAsync(response.Id) as IUserMessage;
        await msg!.ModifyAsync(properties =>
        {
            properties.Cards = properties.Cards.Append(new CardBuilder()
                .AddModule(new DividerModuleBuilder())
                .AddModule(new HeaderModuleBuilder().WithText("ModificationHeader")).Build());
        });
    }

}
