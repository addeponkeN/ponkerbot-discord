using Ponko.DiscordBot.Commands;

namespace Ponko.DiscordBot.Repositories;
public interface ICommandRepository
{
    IEnumerable<IReactCommand> GetReactCommands();
    IEnumerable<IChatCommand> GetChatCommands();

}
