using Discord;
using Discord.WebSocket;
using Ponko.DiscordBot.Common;
using Ponko.DiscordBot.Repositories;

namespace Ponko.DiscordBot;

public sealed class PonkoDiscordHost
{
    internal static CommandFactory _commandFactory = new();
    internal static GuildServices _guildServices = new();
    internal static IGuildRepository _guildRepository = new GuildRepository();

    private readonly DiscordSocketClient _client;
    private readonly IDiscordTokenStore _tokenStore;

    private DefaultLogger _logger = new DefaultLogger();

    public PonkoDiscordHost(IDiscordTokenStore tokenStore)
    {
        _tokenStore = tokenStore;
        var cfg = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
            LogLevel = LogSeverity.Verbose,
        };
        cfg.HandlerTimeout = 1000 * 10;
        _client = new DiscordSocketClient(cfg);

        _ = Start();
    }

    private async Task Start()
    {
        await Task.Delay(200);

        _client.Log += Client_Log;
        _client.JoinedGuild += Client_OnJoinedGuild;
        _client.MessageReceived += Client_MessageReceived;
        _client.LoggedIn += Client_LoggedIn;
        _client.Connected += Client_Connected;
        _client.Ready += Client_Ready;
        _client.Disconnected += Client_Disconnected;
        _client.ReactionAdded += Client_ReactionAdded;
        _client.InteractionCreated += Client_InteractionCreated;

        //string audioStreamUrl = await _yt.GetAudioStreamUrl("nA7ZI34_3tg");
        //_player.SetAudioStream(audioStreamUrl);

        Console.WriteLine("logging in... connecting... readying...");
        string token = _tokenStore.Token;

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        ulong gjengen = 225548816747724810;
        ulong becania = 214099882133159938;
        ulong ponker = 748320103392608327;

        //var guild = _client.GetGuild(becania);
        //await SetGuild(guild);
    }

    private async Task Client_Disconnected(Exception arg)
    {
        Console.WriteLine("disconnected!");
    }

    private async Task Client_Ready()
    {
        Console.WriteLine("ready!");
        var guildSockets = new List<Guild>();

        foreach (var guildSocket in _client.Guilds)
        {
            guildSockets.Add(CreateGuild(guildSocket));
        }

        _guildRepository.InitGuilds(guildSockets);
    }

    private async Task Client_Connected()
    {
        Console.WriteLine("connected!");
    }

    private async Task Client_LoggedIn()
    {
        Console.WriteLine("logged in!");
    }

    private Guild CreateGuild(SocketGuild socket)
    {
        try
        {
            return new Guild(socket, PonkoDiscord.AppHost.Services);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task Client_OnJoinedGuild(SocketGuild guild)
    {
        _guildRepository.Add(CreateGuild(guild));
    }

    private async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
    {
    }

    private async Task Client_InteractionCreated(SocketInteraction msg)
    {
        try
        {
            if (msg.User.IsBot)
                return;

            var guildChannel = msg.Channel as SocketGuildChannel;
            var guild = _guildRepository.Get(guildChannel.Guild.Id);

            if (guild == null)
                return;

            guild.InteractionReceived(msg);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private async Task Client_MessageReceived(SocketMessage msg)
    {
        try
        {
            if (msg.Author.IsBot)
                return;

            var guildChannel = msg.Channel as SocketGuildChannel;
            var guild = _guildRepository.Get(guildChannel.Guild.Id);

            if (guild == null)
                return;

            await guild.MessageReceived(msg);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private async Task Client_Log(LogMessage logMsg)
    {
        _logger.Log($"[DISCORD] {logMsg.Message}", ConsoleColor.Blue);
        if (logMsg.Exception != null)
            _logger.Log($"[DISCORD] {logMsg.Exception}", ConsoleColor.DarkRed);
    }
}
