﻿using Discord.WebSocket;

namespace Ponko.DiscordBot.Commands; 

internal class StopSongCommand : IChatCommand
{
    public Guild Guild { get; set; }

    public string Triggers => "stopperino";

    public async Task MessageReceived(SocketMessage msg, string trigger, string query)
    {

    }
}
