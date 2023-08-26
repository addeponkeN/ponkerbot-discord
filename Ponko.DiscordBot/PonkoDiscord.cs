using Discord;
using Discord.Audio;
using Discord.WebSocket;
using NAudio.Wave;
using Ponko.DiscordBot.Models;
using Ponko.DiscordBot.Repositories;
using Ponko.YT;

namespace Ponko.DiscordBot;

public class PonkoDiscord
{
    private DiscordSocketClient _client;
    private SocketVoiceChannel _channel;

    private SocketGuild _guild;
    private SocketVoiceChannel _voiceChannel;
    private SocketGuildChannel _textChannel;

    IGuildRepository _guildRepository;

    public async Task Start()
    {
        Console.WriteLine("starting");

        string appRoot = Directory.GetCurrentDirectory();
        const string tokenFilename = "discordtoken.txt";

        string tokenPath = Path.Combine(appRoot, tokenFilename);

        if(!File.Exists(tokenPath))
        {
            Console.WriteLine("sorry sir/mam, no token found!! ('discordtoken.txt' containing the discord token)");
            Console.ReadLine();
            return;
        }
        
        var readToken = File.ReadAllTextAsync(tokenPath);

        _guildRepository = new GuildRepository();

        var cfg = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
            //LogLevel = LogSeverity.Debug,
        };

        _client = new DiscordSocketClient(cfg);

        _client.Log += Client_Log;
        _client.UserVoiceStateUpdated += ClientOnUserVoiceStateUpdated;
        _client.JoinedGuild += ClientOnJoinedGuild;
        _client.MessageReceived += Client_MessageReceived;
        _client.LoggedIn += Client_LoggedIn;
        _client.Connected += Client_Connected;
        _client.Ready += Client_Ready;

        //string audioStreamUrl = await _yt.GetAudioStreamUrl("nA7ZI34_3tg");
        //_player.SetAudioStream(audioStreamUrl);

        await Console.Out.WriteLineAsync("logging in... connecting... readying...");
        await _client.LoginAsync(TokenType.Bot, await readToken);
        await _client.StartAsync();


        ulong gjengen = 225548816747724810;
        ulong becania = 214099882133159938;
        ulong ponker = 748320103392608327;

        //var guild = _client.GetGuild(becania);
        //await SetGuild(guild);

        await Task.Delay(-1);

        await _client.LogoutAsync();
        await Console.Out.WriteLineAsync("logged out, bye");
        await Task.Delay(2000);
    }

    private async Task Client_Ready()
    {
        await Console.Out.WriteLineAsync("ready!");
        var guildSockets = new List<Guild>();
        foreach (var guildSocket in _client.Guilds)
        {
            //if (guildSocket.Id == 748320103392608327)
            guildSockets.Add(new Guild(guildSocket));
        }

        _guildRepository.InitGuilds(guildSockets);
    }

    private async Task Client_Connected()
    {
        await Console.Out.WriteLineAsync("connected!");
    }

    private async Task Client_LoggedIn()
    {
        await Console.Out.WriteLineAsync("logged in!");
    }

    private void AddStack(Func<Task> action)
    {
        var th = new Thread(() => action());
        th.IsBackground = true;
        th.Start();
    }
    private async Task ClientOnJoinedGuild(SocketGuild guild)
    {
        _guildRepository.Add(new Guild(guild));
    }

    private async Task ClientOnUserVoiceStateUpdated(SocketUser arg1, SocketVoiceState oldState, SocketVoiceState state)
    {
        if (oldState.VoiceChannel != null && state.VoiceChannel != null)
        {
            var guild = state.VoiceChannel.Guild;
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

            guild.MessageReceived(msg);
        }
        catch (Exception e)
        {
            await Console.Out.WriteLineAsync(e.ToString());
        }
    }

    private async Task Client_Log(LogMessage logMsg)
    {
        await Console.Out.WriteLineAsync(logMsg.Message);
    }
}