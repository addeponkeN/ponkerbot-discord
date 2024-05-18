using Discord.Audio;
using Discord.WebSocket;
using NAudio.Wave;
using Ponko.YT;

namespace Ponko.DiscordBot.Common;

public interface IAudioSource
{
    string StreamUrl { get; }
    SocketVoiceChannel VoiceChannel { get; }
}

struct StreamData
{
    public long Position;
    public string AudioUrl;
}

public class AudioManager
{
    private readonly ILogger _logger;
    private readonly IAudioClientManager _audioClientManager;
    private readonly PonkoStreamPlayer _player;

    private AudioOutStream _discStream;
    private MediaFoundationReader _streamReader;
    private CancellationTokenSource _token;

    private bool _paused;
    StreamData _streamData;

    public bool IsPlaying => _player.IsPlaying;

    public AudioManager(ILogger logger, IAudioClientManager audioClientManager,
        PonkoStreamPlayer player)
    {
        _logger = logger;
        _audioClientManager = audioClientManager;
        _player = player;

        audioClientManager.Disconnected += AudioClientManager_Disconnected;
    }

    private async Task AudioClientManager_Disconnected(Exception arg)
    {
        if (_discStream == null)
            return;

        _discStream.Close();
        _ = _discStream.DisposeAsync();
        _discStream = null;
    }

    public void Disconnect()
    {
        if (_discStream == null)
            return;

        StopStream();

        _discStream.Close();
        _ = _discStream.DisposeAsync();
        _discStream = null;
    }

    public void StopStream()
    {
        _token?.Cancel();

        if (_streamReader != null)
        {
            _player.ClearStream();
            _streamReader = null;
        }
    }

    public void Pause()
    {
        _token?.Cancel();

        if (_streamReader != null && !string.IsNullOrEmpty(_streamData.AudioUrl))
        {
            _paused = true;
            _streamData = new StreamData
            {
                Position = _streamReader.Position,
                AudioUrl = _streamData.AudioUrl,
            };
            _logger.Log("pausing");
        }
        else
        {
            _logger.Log("pause failed");
        }
    }

    public async Task ContinuePlay()
    {
        if (!_paused)
        {
            _logger.Log("ContinuePlay() - was not pasued");
            return;
        }

        if (string.IsNullOrEmpty(_streamData.AudioUrl))
        {
            _logger.Log("ContinuePlay() - no audio url");
            return;
        }

        if (_streamReader == null || _streamReader.Length - 1000 < _streamReader.Position)
        {
            _logger.Log("not continuing stream ..");
            return;
        }

        _discStream = await GetOutStream(_audioClientManager.Socket);
        //_streamReader = await GetStreamReader(_streamData.AudioUrl);
        //_streamReader.Position = _streamData.Position;

        _logger.Log("continuing play ..");

        await SendStream();
    }

    public async Task PlayAudio(IAudioSource audioSource, int startTime = 1)
    {
        string streamUrl = audioSource.StreamUrl;

        if (string.IsNullOrEmpty(streamUrl))
        {
            _logger.Log("PlayAudio() streamUrl was empty");
            return;
        }

        _discStream = await GetOutStream(audioSource.VoiceChannel);
        _streamReader = await GetStreamReader(audioSource.StreamUrl);

        if(startTime > 1)
        {
            _streamReader.CurrentTime = TimeSpan.FromSeconds(startTime);
        }

        await SendStream();
    }

    private async Task SendStream()
    {
        try
        {
            if (_token != null && !_token.IsCancellationRequested)
            {
                _token.Cancel();
            }

            await Task.Delay(10);

            _token = new CancellationTokenSource();

            int channels = 2;
            int sampleRate = 48000;
            int bits = 16;

            if (_streamReader.WaveFormat.Channels != channels ||
                _streamReader.WaveFormat.SampleRate != sampleRate ||
                _streamReader.WaveFormat.BitsPerSample != bits)
            {
                var conversionStream = new WaveFormatConversionStream(new WaveFormat(sampleRate, bits, channels), _streamReader);
                await Console.Out.WriteLineAsync("copying (conversion)...");
                await conversionStream.CopyToAsync(_discStream, _token.Token);
            }
            else
            {
                await Console.Out.WriteLineAsync("copying...");
                await _streamReader.CopyToAsync(_discStream, _token.Token);
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"SendStream() || {e.Message}\n{e}");
        }
        finally
        {
            _token = null;
        }
    }

    private async Task<MediaFoundationReader> GetStreamReader(string streamUrl)
    {
        _token?.Cancel();

        _logger.Log($" -- Getting stream from URL ... --");
        _streamReader = await _player.CreateStream(streamUrl);
        _logger.Log($" -- GOT STREAM --");

        int maxRetries = 10;
        int count = 1;
        while (_streamReader == null)
        {
            if (count >= maxRetries) return null;
            int retryWaitTime = Math.Min(1500, 250 * count);
            _logger.LogWarning($"Failed to get StreamReader for URL: {streamUrl}  ||  Retrying in {retryWaitTime}ms ...");
            Thread.Sleep(retryWaitTime);
            _streamReader = await _player.CreateStream(streamUrl);
            count++;
            if (_streamReader != null)
                _logger.Log($"Got stream for URL '{streamUrl}' after {count} retries.");
        }

        _streamData.AudioUrl = streamUrl;

        return _streamReader;
    }

    private async Task<AudioOutStream> GetOutStream(SocketVoiceChannel channel)
    {
        if (!_audioClientManager.IsVoiceConnected)
        {
            _ = (_discStream?.DisposeAsync());
            _discStream = null;
            await _audioClientManager.ConnectVoice(channel);
        }

        _discStream ??= _audioClientManager.Client.CreatePCMStream(AudioApplication.Music);
        return _discStream;
    }
}