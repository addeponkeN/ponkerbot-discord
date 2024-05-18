using Discord.Audio;
using Discord.WebSocket;

namespace Ponko.DiscordBot.Common;

public interface IAudioClientManager
{
    public IAudioClient Client { get; set; }
    bool IsVoiceConnected { get; }
    SocketVoiceChannel Socket { get; }
    Task ConnectVoice(SocketVoiceChannel channel);

    event Func<Exception, Task> Disconnected;
    event Func<SocketVoiceChannel, Task> Connected;
}

public class AudioClientManager : IAudioClientManager
{
    private readonly ILogger _logger;

    public event Func<Exception, Task> Disconnected;
    public event Func<SocketVoiceChannel, Task> Connected;

    public SocketVoiceChannel Socket { get; private set; }
    public IAudioClient Client { get; set; }
    public bool IsVoiceConnected => Client != null && Client.ConnectionState == Discord.ConnectionState.Connected;

    public AudioClientManager(ILogger logger)
    {
        _logger = logger;
    }

    public async Task ConnectVoice(SocketVoiceChannel channel)
    {
        if (Client != null)
        {
            if (Client.ConnectionState == Discord.ConnectionState.Connected)
            {
                await Console.Out.WriteLineAsync("ConnectVoice() - already connected to voice");
                return;
            }

            Client.Connected -= Client_Connected;
            Client.Disconnected -= Client_Disconnected;

            await Client.StopAsync();

            Client.Dispose();
            Client = null;
        }

        Socket = channel;

        _logger.Log("connecting to voice");
        Client = await channel.ConnectAsync(true);

        if (Client != null)
        {
            Client.Connected += Client_Connected;
            Client.Disconnected += Client_Disconnected;
        }

        _logger.Log("conencting to voice!");

        if (Client == null || Client.ConnectionState != Discord.ConnectionState.Connected)
        {
            _logger.LogWarning($"ConnectVoice() something wrong?  ClientNull: {Client == null} | State: {Client?.ConnectionState}");
        }
    }

    private async Task Client_Connected()
    {
        Connected?.Invoke(Socket);
    }

    private async Task Client_Disconnected(Exception arg)
    {
        _logger.LogError($"disconnected from voice!\n{arg.Message}\n{arg}\n\n-- Reconnecting to voice...");
        Disconnected?.Invoke(arg);
    }
}
