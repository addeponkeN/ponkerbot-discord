using Discord.WebSocket;

namespace Ponko.DiscordBot.Commands; 

internal class StopSongCommand : CommandBase
{
    public override bool CanExecute()
    {
        return true;
    }

    public override void Execute(SocketMessage msg, string trigger, string query)
    {

    }
}
