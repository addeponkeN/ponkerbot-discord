using Discord;
using Discord.WebSocket;
using Ponko.DiscordBot.Common;
using Ponko.MediaLib;
using Ponko.MusicFinder;
using Ponko.Util;
using Ponko.YT;
using System.Text;

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
            _current = value;
            SongChangedEvent?.Invoke(_current);
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

public class Song : IAudioSource
{
    public string Title { get; set; }
    public string Artist { get; set; }
    public string Url { get; set; }
    public string StreamUrl { get; set; }
    public int Duration { get; set; }
    public int StartDuration { get; set; }
    public SocketVoiceChannel VoiceChannel { get; set; }
    public SocketTextChannel TextChannel { get; set; }
    public DateTime TimeQueued { get; set; }
    public DateTime TimePlaying { get; set; }

    public override string ToString()
    {
        return $"{Title} ({Url})";
    }

    public string ToDisplayString()
    {
        var sb = new StringBuilder();
        sb.Append($"[{Title}]({Url})");
        if (TimePlaying != DateTime.MinValue)
            sb.Append($" (Played at {TimePlaying.ToShortTimeString()})");
        return sb.ToString();
    }

    public string ToTitleUrl()
    {
        return $"[{Title}]({Url})";
    }
    public string ToDurationDisplayString()
    {
        var span = TimeSpan.FromSeconds(Duration);
        if (span.TotalMinutes >= 60)
            return $"{(int)span.TotalHours}:{(int)span.Minutes}:{span.Seconds}";
        return $"{(int)span.TotalMinutes}:{span.Seconds}";
    }

    public string ToPlayedAtString()
    {
        return $"Played at {TimePlaying.ToShortTimeString()}";
    }
}

public class SongRequestCommand : IChatCommand
{
    private readonly IAudioClientManager _audioClientManager;
    private readonly MusicManager _musicManager;
    private readonly MediaPlaylist<Song> _playlist;
    private readonly IPonkoMusicFinder _musicFinder;
    private readonly IChatter _chatter;

    public string Triggers => "play,playy,pla,ply,plya,paly,oaly,olay,p";
    public Guild Guild { get; set; }

    public SongRequestCommand(IAudioClientManager audioClientManager,
        MusicManager musicManager,
        MediaPlaylist<Song> playlist,
        IPonkoMusicFinder musicFinder,
        IChatter chatter)
    {
        _audioClientManager = audioClientManager;
        _musicManager = musicManager;
        _playlist = playlist;
        _musicFinder = musicFinder;
        _chatter = chatter;
    }

    public async Task MessageReceived(SocketMessage msg, string commandName, string query)
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

        if (string.IsNullOrEmpty(query))
        {
            var voiceChannel = guildUser.VoiceChannel;
            await _audioClientManager.ConnectVoice(voiceChannel);
            await _musicManager.ContinuePlay();
            return;
        }

        if (!_audioClientManager.IsVoiceConnected)
        {
            //  just sent: .play
            var voiceChannel = guildUser.VoiceChannel;
            await _audioClientManager.ConnectVoice(voiceChannel);
        }

        const string errorMsg = "da,m somuting wong hehe";

        var findResult = await _musicFinder.FindStreamUrlAsync(query);

        if (!findResult.IsSuccess)
        {
            Console.Out.WriteLineAsync(findResult.Title);
            _chatter.Send(textChannel, errorMsg);
            return;
        }

        LinkHelper.TryGetYoutubeTimestamp(query, out int time);

        var song = new Song
        {
            VoiceChannel = guildUser.VoiceChannel,
            Artist = findResult.Artist,
            Title = findResult.Title,
            Url = findResult.LinkUrl,
            StreamUrl = findResult.StreamUrl,
            Duration = findResult.Duration,
            StartDuration = time,
            TextChannel = textChannel,
            TimeQueued = DateTime.Now,
        };

        _chatter.SendSongQueued(textChannel, song, _playlist.QueueCount + 1);

        _playlist.QueueSong(song);

        //string[] links = new string[]
        //{
        //    "https://www.youtube.com/watch?v=jlopzlDeLgI",
        //    "https://www.youtube.com/watch?v=d_MSTG-X4S0",
        //    "https://www.youtube.com/watch?v=Hg9SqDAcB_s",
        //    "https://www.youtube.com/watch?v=EO2lkMIxvUU",
        //    "https://www.youtube.com/watch?v=faZEYN4KHx0",
        //    "https://www.youtube.com/watch?v=F6E8oBp-MjQ",
        //    "https://www.youtube.com/watch?v=cf8PNTBTJZU",
        //};

        //foreach (var link in links)
        //{
        //    findResult = await _musicFinder.FindStreamUrlAsync(link);
        //    song = new Song
        //    {
        //        VoiceChannel = guildUser.VoiceChannel,
        //        Artist = findResult.Artist,
        //        Title = findResult.Title,
        //        Url = findResult.LinkUrl,
        //        StreamUrl = findResult.StreamUrl,
        //        Duration = findResult.Duration,
        //        TextChannel = textChannel,
        //        TimeQueued = DateTime.Now,
        //    };
        //    _playlist.QueueSong(song);
        //}

    }
}

