using Discord.WebSocket;
using Ponko.DiscordBot.Common;
using Ponko.MediaLib;

namespace Ponko.DiscordBot.Commands;

public class CurrentSongInfoCommand : IChatCommand
{
    private readonly IMediaSongStore<Song> _songStore;
    private readonly IChatter _chatter;
    public Guild Guild { get; set; }

    public string Triggers => "current,song,currentsong,songinfo,cr,cs,now,playing";

    public CurrentSongInfoCommand(IMediaSongStore<Song> songStore, IChatter chatter)
    {
        _songStore = songStore;
        _chatter = chatter;
    }

    public async Task MessageReceived(SocketMessage msg, string trigger, string query)
    {
        var song = _songStore.Current;
        var channel = msg.Channel as SocketTextChannel;

        if(channel == null)
        {
            return;
        }

        if (song == null)
        {
            _chatter.Send(channel, "### toto so - bing bing ###");
        }
        else
        {
            _chatter.SendSongInfo(channel, song);
        }
    }
}
