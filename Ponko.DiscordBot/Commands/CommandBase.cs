using Discord.WebSocket;

namespace Ponko.DiscordBot.Commands;

public interface ICommand
{
    Guild Guild { get; set; }
    ulong GuildId => Guild.Id;
    bool CanExecute() => true;
}

public interface IChatCommand : ICommand
{
    string Triggers { get; }
    Task MessageReceived(SocketMessage msg, string trigger, string query);
}

public interface IReactCommand : ICommand
{
    void ReactReceived();
}