using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Ponko.DiscordBot.Common;

namespace Ponko.DiscordBot;

public class Guild
{
    private readonly ILogger _logger;
    private readonly IServiceProvider _provider;
    private readonly CommandHandler _commandHandler;
    private readonly MusicManager _musicManager;
    private readonly GuildPlaylist _guildPlaylist;

    public ulong Id => Socket.Id;
    public SocketGuild Socket { get; }

    public Guild(SocketGuild socket, IServiceProvider provider)
    {
        Socket = socket;
        _provider = provider;

        _logger = _provider.GetRequiredService<ILogger>();

        _guildPlaylist = (GuildPlaylist)_provider.GetRequiredService<IGuildPlaylistRepository>();
        _guildPlaylist.SetGuild(this);

        _musicManager = _provider.GetRequiredService<MusicManager>();

        _commandHandler = _provider.GetRequiredService<CommandHandler>();

    }

    public async Task MessageReceived(SocketMessage message)
    {
        try
        {
            await _commandHandler.HandleMessage(message);
        }
        catch (Exception e)
        {
            LogException(e);
        }
    }

    internal void InteractionReceived(SocketInteraction msg)
    {
        try
        {
            _commandHandler.HandleInteraction(msg);
        }
        catch (Exception e)
        {
            LogException(e);
        }
    }


    private void LogException(Exception e)
    {
        _logger.LogError($"{nameof(e)} :: {e.Message}\n{e}");
        throw e;
    }
}