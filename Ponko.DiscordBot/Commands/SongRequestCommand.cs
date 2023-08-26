using AngleSharp.Html.Dom;
using Discord;
using Discord.WebSocket;
using Ponko.HtmlExtensions;
using Ponko.MediaLib;
using Ponko.MusicFinder;
using Ponko.SoundCloud;
using Ponko.YoutubeApi;
using Ponko.YT;
using System.Text.RegularExpressions;

namespace Ponko.DiscordBot.Commands;

public class SongStore : IMediaSongStore<Song>
{
    private readonly PonkoStreamPlayer _player;

    private Song _current;
    public Song? Current
    {
        get => _current;
        set
        {
            if (_current == value) return;
            SongChangedEvent?.Invoke(value);
            _current = value;
        }
    }

    public TimeSpan TotalTime => _player.TotalTime;

    public TimeSpan CurrentTime { get => _player.CurrentTime; set => _player.CurrentTime = value; }

    public float TimeLeftSeconds => (float)(TotalTime.TotalSeconds - CurrentTime.TotalSeconds);

    public event Action<Song?> SongChangedEvent;

    public SongStore(PonkoStreamPlayer player)
    {
        _player = player;
    }
}

public class Song
{
    public string Title { get; set; }
    public string Url { get; set; }
    public string AudioStreamUrl { get; set; }
    public int Duration { get; set; }
    public SocketVoiceChannel VoiceChannel { get; set; }

    public override string ToString()
    {
        return $"{Title} ({Url})";
    }
}

public class SongRequestCommand : CommandBase
{
    private readonly MediaPlaylist<Song> _playlist;
    private readonly PonkoMusicFinder _musicFinder;

    public SongRequestCommand(MediaPlaylist<Song> playlist, PonkoMusicFinder musicFinder)
    {
        _playlist = playlist;
        _musicFinder = musicFinder;

        Triggers.Add("play");
        Triggers.Add("playy");

        Triggers.Add("pla");
        Triggers.Add("ply");
        Triggers.Add("plya");

        Triggers.Add("paly");

        Triggers.Add("oaly");
        Triggers.Add("olay");

        Triggers.Add("p");
    }

    public override bool CanExecute() => true;

    private EmbedBuilder? GetResponseText(Song song, int queueCount)
    {
        string text = $"\n### [{song.Title}](<{song.Url}>)" +
            $"\nQueue: {queueCount}";
        var msg = new EmbedBuilder
        {
            Color = Color.Orange,
            Description = text,
            Title = ":white_check_mark: ok sir coming right up!! :white_check_mark:",
            //Fields = 
            //{ new EmbedFieldBuilder { Name = $"duration", Value = "13:37", }, },
        };

        return msg;
    }

    public override async void Execute(SocketMessage msg, string commandName, string query)
    {
        if (msg.Author is not SocketGuildUser guildUser)
            return;

        var textChannel = msg.Channel as SocketTextChannel;
        if (textChannel == null) return;

        if (guildUser.VoiceChannel == null)
        {
            const string errorNotInChannel = ":interrobang: wee woo alaert (retard) :yum: :interrobang:";
            _ = textChannel.SendMessageAsync(errorNotInChannel);
            return;
        }

        const string errorMsg = "da,m somuting wong hehe";

        var findResult = await _musicFinder.FindStreamUrlAsync(query);

        if (!findResult.IsSuccess)
        {
            await Console.Out.WriteLineAsync(findResult.Title);
            _ = textChannel.SendMessageAsync(errorMsg);
            return;
        }

        var song = new Song
        {
            VoiceChannel = guildUser.VoiceChannel,
            Title = findResult.Title,
            Url = findResult.LinkUrl,
            AudioStreamUrl = findResult.StreamUrl,
        };

        EmbedBuilder response = GetResponseText(song, _playlist.QueueCount + 1)!;
        _ = textChannel.SendMessageAsync(embed: response.Build());

        _playlist.QueueSong(song);

        int i = 0;
        Console.WriteLine($"queued: {song}");
        foreach (var item in _playlist.Queue)
            Console.WriteLine($"({i++}) {item}");
    }
}

