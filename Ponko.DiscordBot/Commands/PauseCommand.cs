using Discord.WebSocket;
using Ponko.DiscordBot.Common;
using Ponko.Util;

namespace Ponko.DiscordBot.Commands;

public class PauseCommand : IChatCommand
{
    private readonly MusicManager _musicManager;

    private static readonly string[] skipResponses = { "pausered.", "TEISHI", "stoppu", ":sob: YAMETE :sob:" };

    public Guild Guild { get; set; }

    public string Triggers => "stop,pause";

    public PauseCommand(MusicManager musicManager)
    {
        _musicManager = musicManager;
    }

    public async Task MessageReceived(SocketMessage msg, string trigger, string query)
    {
        string responseText = skipResponses.RandomItem();
        msg.Channel.SendMessageAsync($"### :musical_note: :pause_button: {responseText} :pause_button: :musical_note:");

        _musicManager.Pause();

        //_musicManager.Disconnect();
    }
}