using Discord.WebSocket;

namespace Ponko.DiscordBot.Commands;

public class TestCommand : IChatCommand
{
    public Guild Guild { get; set; }

    public string Triggers => "test";

    public async Task MessageReceived(SocketMessage msg, string trigger, string query)
    {
        Console.WriteLine("no such command !!");
    }
}
