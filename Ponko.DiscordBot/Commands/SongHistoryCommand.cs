using Discord;
using Discord.WebSocket;
using Ponko.DiscordBot.Common;
using Ponko.MediaLib;
using System.Text;

namespace Ponko.DiscordBot.Commands;

public class SongHistoryCommand : IChatCommand
{
    private readonly MediaPlaylist<Song> _playlist;
    private readonly IChatter _chatter;

    public Guild Guild { get; set; }

    public string Triggers => "history,previous,played";

    public SongHistoryCommand(MediaPlaylist<Song> playlist,
        IChatter chatter)
    {
        _playlist = playlist;
        _chatter = chatter;
    }

    public async Task MessageReceived(SocketMessage msg, string trigger, string query)
    {
        var history = _playlist.History;

        if(history.Count <= 0)
        {
            _chatter.Send(msg.Channel as SocketTextChannel, "no history :(");
            return;
        }

        var reversed = history.Reverse();

        var sb = new StringBuilder();

        int i = 1;
        foreach (var song in reversed)
        {
            if (i > 1) sb.Append('\n');
            sb.Append($"❯{i++}. [{song.Title}]({song.Url}) (Played :watch: {song.TimePlaying.ToString("t")})");
        }

        var embed = _chatter.CreateBuilder(":eight_spoked_asterisk: HISTORY :eight_spoked_asterisk:", sb.ToString()).Build();
        _chatter.Send((msg.Channel as SocketTextChannel)!, embed);
    }
}
