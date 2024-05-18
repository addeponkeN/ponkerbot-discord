using Discord.WebSocket;

namespace Ponko.DiscordBot.Commands;

internal class CrookCommand : IChatCommand
{
    string _triggers = "crook";
    public string Triggers => _triggers;

    public Guild Guild { get; set; }

    public async Task MessageReceived(SocketMessage msg, string trigger, string query)
    {

    }
}
