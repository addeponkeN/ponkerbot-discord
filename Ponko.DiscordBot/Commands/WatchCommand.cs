using Discord.WebSocket;
using Ponko.DiscordBot.Common;

namespace Ponko.DiscordBot.Commands;

public class WatchCommand : IChatCommand
{
    private readonly IChatter _chat;

    public Guild Guild { get; set; }

    public string Triggers => "watch";

    public WatchCommand(IChatter chat)
    {
        _chat = chat;
    }

    public async Task MessageReceived(SocketMessage msg, string trigger, string query)
    {
        //  chatter was not in a channel
        if (msg.Author is not SocketGuildUser guild)
            return;

        //  hm
        if (msg.Channel is not SocketTextChannel textChannel)
            return;

        _chat.Send(msg.Channel as SocketTextChannel, "watch it video !! hehe");
    }
}
