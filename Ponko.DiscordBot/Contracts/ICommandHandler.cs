using Discord.WebSocket;

namespace Ponko.DiscordBot.Contracts;

public interface ICommandHandler
{
    Task HandleMessage(SocketMessage msg);
}