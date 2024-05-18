using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Ponko.DiscordBot.Commands;
using Ponko.DiscordBot.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace Ponko.DiscordBot.Common;

public interface ICommandProvider
{
    IEnumerable<Func<ICommand>> GetCreators<T>();
}

public class CommandProvider : ICommandProvider
{
    private readonly IServiceProvider _provider;

    private readonly Dictionary<Type, IEnumerable<Func<ICommand>>> _allCommands = new();
    private readonly List<Func<IChatCommand>> _chatCommandCreators = new();
    private readonly List<Func<IReactCommand>> _reactionCommandCreators = new();

    public CommandProvider(IServiceProvider provider)
    {
        Init();
        _provider = provider;
    }

    public IEnumerable<Func<ICommand>> GetCreators<T>()
    {
        var type = typeof(T);
        _allCommands.TryGetValue(type, out var commands);
        return commands;
    }

    private void Init()
    {
        _allCommands.Clear();

        _chatCommandCreators.Clear();
        foreach (var chatCommandCreator in GetTypes<IChatCommand>())
            _chatCommandCreators.Add(chatCommandCreator);
        _allCommands.Add(typeof(IChatCommand), _chatCommandCreators);

        _reactionCommandCreators.Clear();
        foreach (var reactCommandCreator in GetTypes<IReactCommand>())
            _reactionCommandCreators.Add(reactCommandCreator);
        _allCommands.Add(typeof(IReactCommand), _reactionCommandCreators);
    }

    private IEnumerable<Func<T>> GetTypes<T>() where T : ICommand
    {
        var type = typeof(T);
        var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes());
        var filteredTypes = allTypes.Where(x => !x.IsAbstract && x.IsClass && type.IsAssignableFrom(x));

        foreach (var filteredType in filteredTypes)
        {
            var creator = () => (T)_provider.GetRequiredService(filteredType);
            yield return creator;
        }
    }
}

public class CommandHandler : ICommandHandler
{
    private readonly ICommandValidator _commandValidator = new CommandValidator();
    private readonly IServiceProvider _provider;
    private readonly ICommandProvider _cmdProvider;

    private Dictionary<IChatCommand, HashSet<string>> _commandTriggers = new();
    private List<IChatCommand> _chatCommands = new();

    private TestCommand _fallbackCommand = new();

    public CommandHandler(IServiceProvider provider, ICommandProvider cmdProvider)
    {
        _provider = provider;
        _cmdProvider = cmdProvider;
        _provider = PonkoDiscord.AppHost.Services;

        Init();
    }

    private void Init()
    {
        _chatCommands.Clear();
        foreach (var chatCommand in _cmdProvider.GetCreators<IChatCommand>())
        {
            var command = (IChatCommand)chatCommand();
            Add(command);
        }
    }

    private IChatCommand GetCommand(string trigger)
    {
        trigger = trigger.ToLower();
        for (int i = 0; i < _chatCommands.Count; i++)
        {
            var cmd = _chatCommands[i];
            var triggers = _commandTriggers[cmd];
            if (triggers.Contains(trigger))
                return cmd;
        }

        return _fallbackCommand;
    }

    public async Task HandleMessage(SocketMessage msg)
    {
        if (msg is not IUserMessage message) return;

        if (_commandValidator.IsValidCommand(msg.Content))
        {
            var split = message.Content.Split(' ', 2);
            string commandFullString = split[0];
            string commandTriggerer = commandFullString[1..];
            string query = split.Length > 1 ? split[1] : string.Empty;

            var command = GetCommand(commandTriggerer);
            _ = command?.MessageReceived(msg, commandTriggerer, query)!;
        }
    }

    public void Add(IChatCommand cmd)
    {
        string triggers = cmd.Triggers;
        _chatCommands.Add(cmd);

        var splitTriggers = triggers.Trim().Split(',').ToHashSet();
        _commandTriggers.Add(cmd, splitTriggers);
    }

    internal void HandleInteraction(SocketInteraction msg)
    {
    }
}