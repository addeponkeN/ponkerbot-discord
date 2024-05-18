using Discord.WebSocket;
using Ponko.MediaLib;
using Ponko.Util;

namespace Ponko.DiscordBot.Commands;

public class SkipSongCommand : IChatCommand
{
    private readonly MediaPlaylist<Song> _mediaPlaylist;

    private static readonly string[] _skipResponses = { "skipperoni", "skipperino", "skip:P",
        "skippser", "nexting", "go next", "TSUGI", "skippu", "Sukippu", "yebyeb", "ok.." };

    public Guild Guild { get; set; }

    public string Triggers => "skip,skipå,skipo,skiop,skop,skpi,skjip,sikp,next";

    public SkipSongCommand(MediaPlaylist<Song> mediaPlaylist)
    {
        _mediaPlaylist = mediaPlaylist;
    }

    public async Task MessageReceived(SocketMessage msg, string commandName, string query)
    {
        string responseText = _skipResponses.RandomItem();
        msg.Channel.SendMessageAsync($"### :musical_note: :track_next: {responseText} :track_next: :musical_note:");
        _mediaPlaylist.Next();
    }
}
