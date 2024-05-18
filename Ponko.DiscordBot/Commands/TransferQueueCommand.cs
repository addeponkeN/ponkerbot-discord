using Discord.WebSocket;
using Ponko.DiscordBot.Common;
using Ponko.MediaLib;

namespace Ponko.DiscordBot.Commands;

internal class TransferQueueCommand : IChatCommand
{
    private readonly IChatter _chatter;
    private readonly MediaPlaylist<Song> _playlist;

    public Guild Guild { get; set; }

    public string Triggers => "transfer";

    public TransferQueueCommand(IChatter chatter, MediaPlaylist<Song> playlist)
    {
        _chatter = chatter;
        _playlist = playlist;
    }

    public async Task MessageReceived(SocketMessage msg, string trigger, string query)
    {
        var textChannel = msg.Channel as SocketTextChannel;
        if(textChannel == null)
            return;

        var queue = _playlist.Queue;

        if(queue.Count <= 0)
        {
            return;
        }

        string prefix = query;

        if (string.IsNullOrEmpty(prefix))
        {
            _chatter.Send(textChannel, "Invalid !!\n>.transfer ,play");
            return;
        }

        foreach (var song in queue)
        {
            _chatter.Send(textChannel, $"{prefix} {song.Title}");
            await Task.Delay(100);
        }

    }
}
