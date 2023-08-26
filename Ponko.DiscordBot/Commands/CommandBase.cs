
using Discord.WebSocket;
using Ponko.DiscordBot.Models;

namespace Ponko.DiscordBot.Commands;

public abstract class CommandBase
{
    public Guild Guild { get; set; }
    public HashSet<string> Triggers { get; set; } = new();

    public bool IsTriggerMatch(string command) => Triggers.Contains(command);
    public virtual bool CanExecute() => true;
    public abstract void Execute(SocketMessage msg, string trigger, string query);

}
