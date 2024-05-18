using Discord.WebSocket;
using Ponko.DiscordBot.Common;
using Ponko.MediaLib;
using System.Text;

namespace Ponko.DiscordBot.Commands;

public class SongQueueListCommand : IChatCommand
{
    private readonly IChatter _chatter;
    private readonly MediaPlaylist<Song> _playlist;

    public Guild Guild { get; set; }

    public string Triggers => "queue,songs,queued,queuelist,list,songlist,queued";

    public SongQueueListCommand(IChatter chatter, MediaPlaylist<Song> playlist)
    {
        _chatter = chatter;
        _playlist = playlist;
    }

    public async Task MessageReceived(SocketMessage msg, string trigger, string query)
    {
        var queue = _playlist.Queue;

        if(queue.Count <= 0)
        {
            _chatter.Send(msg.Channel as SocketTextChannel, "no queued :(");
            return;
        }

        var sb = new StringBuilder();

        int i = 1;
        foreach (var song in queue)
        {
            if (i > 1) sb.Append('\n');
            sb.Append($"❯{i++}. {song.ToTitleUrl()}");
        }

        var embed = _chatter.CreateBuilder(":parking: QUEUE :parking:", sb.ToString()).Build();

        _chatter.Send((msg.Channel as SocketTextChannel)!, embed);
    }
}
