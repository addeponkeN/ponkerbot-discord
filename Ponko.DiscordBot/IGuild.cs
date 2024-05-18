using Discord.WebSocket;

namespace Ponko.DiscordBot;

public interface IGuild
{
    void MessageReceived(SocketMessage message);
}
