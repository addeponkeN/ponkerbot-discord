using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Ponko.DiscordBot.Commands;
using Ponko.MediaLib;
using Ponko.YT;
using System.Text;

namespace Ponko.DiscordBot.Common;

public interface IChatter
{
    Task<RestUserMessage> Send(SocketTextChannel channel, string msg);
    void Send(SocketTextChannel channel, Embed embed);
    void SendSongPlaying(SocketTextChannel channel, Song song);
    void SendSongQueued(SocketTextChannel channel, Song song, int queueId);
    void SendSongInfo(SocketTextChannel channel, Song song);

    EmbedBuilder CreateBuilder(string title, string desc);
    EmbedBuilder CreateSongBuilder(string title, string desc, Song song);
}

public class Chatter : IChatter
{
    private readonly MediaPlaylist<Song> _playlist;
    private readonly PonkoStreamPlayer _player;

    private SocketTextChannel _playingMsgChannel;
    private RestUserMessage _playingMsg;

    public Chatter(MediaPlaylist<Song> playlist, PonkoStreamPlayer player)
    {
        _playlist = playlist;
        _player = player;
    }

    public async Task<RestUserMessage> Send(SocketTextChannel channel, string msg)
    {
        var res = await channel.SendMessageAsync(msg);
        return res;
    }

    public void Send(SocketTextChannel channel, Embed embed)
    {
        _ = channel.SendMessageAsync(embed: embed);
    }

    public void SendSongPlaying(SocketTextChannel channel, Song song)
    {
        if (_playingMsg != null)
        {
            _playingMsgChannel.DeleteMessageAsync(_playingMsg);
        }

        _playingMsgChannel = channel;

        var buttonBuilder = new ButtonBuilder(style: ButtonStyle.Secondary, label: "next", customId: "nextbutton");
        var compBuilder = new ComponentBuilder();
        compBuilder.WithButton(buttonBuilder);
        var comp = compBuilder.Build();

        var embed = GetPlayingSongCardEmbed(song);
        _ = channel.SendMessageAsync(embed: embed, components: comp)
            .ContinueWith(msg =>
            {
                _playingMsg = msg.Result;
            });
    }

    public void SendSongQueued(SocketTextChannel channel, Song song, int queueId)
    {
        var embed = GetQueuedSongCardEmbed(song, queueId);
        _ = channel.SendMessageAsync(embed: embed);
    }

    public void SendSongInfo(SocketTextChannel channel, Song song)
    {
        var embed = GetSongInfoCardEmbed(song);
        _ = channel.SendMessageAsync(embed: embed);
    }

    public EmbedBuilder CreateBuilder(string title, string desc)
    {
        desc = FormatCardText(desc);
        //desc = desc.Replace("amp;", string.Empty);
        //desc = desc.Replace("\"", string.Empty);
        //desc = desc.Replace("'", string.Empty);

        return new EmbedBuilder
        {
            Color = Color.Orange,
            Title = $"{title}",
            Description = desc,

        };
    }

    private string FormatCardText(string text)
    {
        text = text.Replace("amp;", string.Empty);
        //text = text.Replace("\"", string.Empty);
        //text = text.Replace("'", string.Empty);
        return text;
    }

    public EmbedBuilder CreateSongBuilder(string title, string desc, Song song)
    {
        var builder = CreateBuilder(title, desc);
        return builder;
    }

    private Embed GetQueuedSongCardEmbed(Song song, int queueCount)
    {
        string songTitle = FormatCardText(song.Title);
        string text =
            $"\n### [{songTitle}](<{song.Url}>)" +
            $"\nLength: {GetTimeFormat(song.Duration)}" +
            $"\nQueue: {queueCount}";

        var builder = CreateSongBuilder(":white_check_mark: QUEUED :white_check_mark:", text, song);

        return builder.Build();
    }

    private Embed GetPlayingSongCardEmbed(Song song)
    {
        var history = _playlist.History.TakeLast(3);
        var queue = _playlist.Queue.Take(2);
        var current = song;

        var sb = new StringBuilder();

        //if (!queue.Any() && history.Count() <= 1)
        //{
        //    sb.Append($"### {current.ToDisplayString()}\n");
        //}
        //else
        //{
        //foreach (var item in history)
        //    if (item == song)
        //    else
        //        sb.Append($"> ~~{item.ToTitleUrl()}~~ ({item.ToPlayedAtString()})\n");
        sb.Append($"### ❯ {current.ToTitleUrl()}\n");
        sb.Append($"{current.ToDurationDisplayString()}");

        //if (queue.Any())
        //    sb.Append("\nNext:\n");
        //var next = _playlist.Queue.FirstOrDefault();
        //if (next != null)
        //{
        //    sb.Append($"> Next: {next.ToTitleUrl()}\n");
        //}
        //foreach (var item in queue)
        //sb.Append($"> {item.ToDisplayString()}\n");
        //}

        string text = sb.ToString();

        var builder = CreateSongBuilder(":arrow_forward: PLAYING :arrow_forward:", text, song);

        return builder.Build();
    }

    private Embed GetSongInfoCardEmbed(Song song)
    {
        string strDuration = "";

        if (_player == null)
        {
            strDuration = song.ToDurationDisplayString();
        }
        else
        {
            var elapsed = _player.CurrentTime;
            strDuration = $"({(int)elapsed.TotalMinutes}:{elapsed.Seconds}/{song.ToDurationDisplayString()})";
        }

        string text =
            $"\n### ❯ [{song.Title}](<{song.Url}>)" +
            $"\n{strDuration}";

        var builder = CreateSongBuilder(":musical_note: CURRENT :musical_note:", text, song);

        return builder.Build();
    }

    private string GetTimeFormat(int duration)
    {
        var span = TimeSpan.FromSeconds(duration);
        return $"{(int)Math.Floor(span.TotalMinutes)}:{span.Seconds}";
    }
}
