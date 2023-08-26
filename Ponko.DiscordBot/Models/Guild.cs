using Discord.Audio;
using Discord.WebSocket;
using Ponko.DiscordBot.Commands;
using Ponko.DiscordBot.Services;
using Ponko.MediaLib;
using Ponko.MusicFinder;
using Ponko.SoundCloud;
using Ponko.YoutubeApi;
using Ponko.YT;

namespace Ponko.DiscordBot.Models;

public class Guild
{
    private CommandManager _commandManager;

    public ulong Id => Socket.Id;
    public SocketGuild Socket { get; }
    public IAudioClient AudioClient { get; private set; }
    public bool IsVoiceConnected => AudioClient != null && AudioClient.ConnectionState == Discord.ConnectionState.Connected;

    private YT.PonkoStreamPlayer _player;
    private MusicManager _musicManager;
    private MediaPlaylist<Song> _mediaPlaylist;
    private SongStore _songStore;
    private PonkoYtApi _youtubeApi;
    private PonkoYT _yt;
    private PonkoSoundCloud _soundCloud;
    private PonkoMusicFinder _musicFinder;

    public Guild(SocketGuild socket)
    {
        Socket = socket;

        _soundCloud = new();
        _yt = new();
        _youtubeApi = new();
        _youtubeApi.Start();
        _player = new YT.PonkoStreamPlayer();
        _songStore = new SongStore(_player);
        _mediaPlaylist = new MediaPlaylist<Song>(_songStore);
        _musicFinder = new();
        _musicManager = new(this, _player, _mediaPlaylist, _songStore, _yt);

        _commandManager = new(this);
        _commandManager.AddCommand(new SongRequestCommand(_mediaPlaylist, _musicFinder));
        _commandManager.AddCommand(new SkipSongCommand(_mediaPlaylist, _musicManager));
    }

    public void MessageReceived(SocketMessage message)
    {
        _commandManager.HandleMessage(message);
    }

    public async Task ConnectVoice(SocketVoiceChannel channel)
    {
        await Console.Out.WriteLineAsync("connecting to voice...");
        AudioClient = await channel.ConnectAsync(true);
        await Console.Out.WriteLineAsync("connected to voice!");
    }
}