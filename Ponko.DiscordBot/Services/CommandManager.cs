using Discord;
using Discord.WebSocket;
using Ponko.DiscordBot.Commands;
using Ponko.DiscordBot.Contracts;
using Ponko.DiscordBot.Models;

namespace Ponko.DiscordBot.Services;

public class CommandManager : ICommandManager
{
    private readonly List<CommandBase> _commands = new();
    private readonly ICommandValidator _commandValidator;
    private readonly Guild _guild;

    public CommandManager(Guild guild)
    {
        _guild = guild;
        _commandValidator = new CommandValidator();
    }

    public void AddCommand(CommandBase command)
    {
        command.Guild = _guild;
        _commands.Add(command);
    }

    public void HandleMessage(SocketMessage msg)
    {
        if (msg is not IUserMessage message) return;

        if (_commandValidator.IsValidCommand(msg.Content))
        {
            var split = message.Content.Split(' ', 2);
            string commandFullString = split[0];
            string commandString = commandFullString.Substring(1, commandFullString.Length - 1);
            string query = split.Length > 1 ? split[1] : string.Empty;
            foreach (var command in _commands)
            {
                if (!command.CanExecute())
                    continue;

                if (command.IsTriggerMatch(commandString))
                {
                    command.Execute(msg, commandString, query);
                    Console.WriteLine($"executed command: {command}  ({commandFullString})");
                }
            }
        }
    }
}