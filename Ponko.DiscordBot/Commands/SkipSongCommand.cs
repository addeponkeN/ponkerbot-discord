using Discord.WebSocket;
using Ponko.DiscordBot.Services;
using Ponko.MediaLib;
using Ponko.Util;

namespace Ponko.DiscordBot.Commands;

public class SkipSongCommand : CommandBase
{
    private readonly MediaPlaylist<Song> _mediaPlaylist;
    private readonly MusicManager _musicManager;

    static readonly string[] skipResponses = {"skipperoni", "skipperino", "skip:P",
    "skippser", "nexting", "go next", "TSUGI", "skippu", "Sukippu", "yebyeb", "ok.."};

    public SkipSongCommand(MediaPlaylist<Song> mediaPlaylist, MusicManager musicManager)
    {
        _mediaPlaylist = mediaPlaylist;
        _musicManager = musicManager;
        Triggers.Add("skip");
        Triggers.Add("skpi");
        Triggers.Add("sikp");
        Triggers.Add("skipo");
        Triggers.Add("skiop");
        Triggers.Add("skipå");
    }

    public override bool CanExecute()
    {
        return true;
    }

    public override void Execute(SocketMessage msg, string commandName, string query)
    {
        Console.WriteLine("skipped song!!");

        string responseText = skipResponses.RandomItem();
        msg.Channel.SendMessageAsync($"### :track_next: {responseText} :track_next:");
        _mediaPlaylist.Next();
    }
}
