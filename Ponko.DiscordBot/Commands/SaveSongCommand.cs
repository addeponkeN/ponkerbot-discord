using Discord.WebSocket;
using Ponko.DiscordBot.Common;

namespace Ponko.DiscordBot.Commands;

public class SaveSongCommand : IChatCommand
{
    private readonly IGuildPlaylistRepository _repo;
    private readonly IChatter _chat;

    public Guild Guild { get; set; }

    public string Triggers => "save";

    public SaveSongCommand(IGuildPlaylistRepository repo, IChatter chat)
    {
        _repo = repo;
        _chat = chat;
    }

    public async Task MessageReceived(SocketMessage msg, string trigger, string query)
    {
        var chatChannel = msg.Channel as SocketTextChannel;
        _chat.Send(chatChannel, "he he save!");
    }
}
