using Discord.WebSocket;
using Ponko.DiscordBot.Common;
using System.Threading.Tasks;

namespace Ponko.DiscordBot.Commands;

internal class RollCommand : IChatCommand
{
    private readonly IChatter _chatter;

    public string Triggers => "roll,rol,rlol";

    public Guild Guild { get; set; }

    public RollCommand(IChatter chatter)
    {
        _chatter = chatter;
    }

    public async Task MessageReceived(SocketMessage msg, string trigger, string query)
    {
        var channel = msg.Channel as SocketTextChannel;

        if (channel == null)
        {
            return;
        }

        var roll = Random.Shared.Next(101);

        await _chatter.Send(channel, $"# {roll}");

    }
}
