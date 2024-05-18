using Ponko.DiscordBot.Commands;
using Ponko.DiscordBot.Common;

namespace Ponko.DiscordBot.Repositories;

public class CommandRepository : ICommandRepository
{
    private List<IChatCommand> _chatCommands;
    private List<IReactCommand> _reactCommands;

    public CommandRepository(ICommandProvider cmdProvider)
    {
        CmdProvider = cmdProvider;
    }

    public ICommandProvider CmdProvider { get; }

    private List<T> InitCommandList<T>() where T : ICommand
    {
        var list = new List<T>();
        var commands = CmdProvider.GetCreators<T>();
        foreach (Func<ICommand> command in commands)
        {
            var obj = command();
            list.Add((T)obj);
        }

        return list;
    }

    public IEnumerable<IChatCommand> GetChatCommands()
        => _chatCommands ??= InitCommandList<IChatCommand>();

    public IEnumerable<IReactCommand> GetReactCommands()
        => _reactCommands ??= InitCommandList<IReactCommand>();
}

