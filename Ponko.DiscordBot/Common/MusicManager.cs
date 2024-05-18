using Discord;
using Discord.Audio;
using Discord.WebSocket;
using NAudio.Wave;
using Ponko.DiscordBot.Commands;
using Ponko.MediaLib;
using Ponko.YT;

namespace Ponko.DiscordBot.Common;

public interface IGuildService
{
    void OnCreate(Guild g);
}

public class MusicManager
{
    private readonly ILogger _logger;
    private readonly IChatter _chatter;
    private readonly PonkoYT _pyt;
    private readonly AudioManager _audioManager;
    private readonly MediaPlaylist<Song> _playlist;
    private readonly SongStore _songStore;

    private SocketTextChannel _recentTextChannel;

    bool _disconnectPause;

    public MusicManager(
        ILogger logger,
        IChatter chatter,
        IAudioClientManager audioClientManager,
        PonkoYT pyt,
        AudioManager audioManager,
        PonkoStreamPlayer player,
        MediaPlaylist<Song> playlist,
        SongStore songStore)
    {
        _logger = logger;
        _chatter = chatter;
        _pyt = pyt;
        _audioManager = audioManager;
        _playlist = playlist;
        _songStore = songStore;

        songStore.SongChangedEvent += SongStore_SongChangedEvent;
        player.PlaybackFinished += Player_PlaybackFinished;
        audioClientManager.Connected += Client_Connected;
        audioClientManager.Disconnected += Client_Disconnected;
    }

    private async Task Client_Disconnected(Exception arg)
    {
        _logger.Log("auto pause on Disconnected");
        _disconnectPause = true;
        Pause();

    }

    private async Task Client_Connected(SocketVoiceChannel channel)
    {
        if (_disconnectPause)
        {
            _logger.Log("auto play on Connected");
            _disconnectPause = false;
            ContinuePlay();
        }
    }

    private async void SongStore_SongChangedEvent(Song? song)
    {
        //StopStream();

        //  find random song
        if (song == null)
        {
            _audioManager.Disconnect();

            await Task.Delay(100);

            if (_playlist.IsAtEndOfPlaylist())
            {
                var first = _playlist.List.LastOrDefault();
                bool firstIsYoutube = first.Url.Contains("youtube") || first.Url.Contains("youtu.be");
                if (firstIsYoutube)
                {
                    await Console.Out.WriteLineAsync($"auto song from: {first.Title}");

                    string videoId = first.Url.Substring(first.Url.Length - 11);
                    var nextVideoId = _pyt.GetRelatedVideo(videoId).Result;

                    if (string.IsNullOrEmpty(nextVideoId))
                    {
                        return;
                    }

                    Console.WriteLine($"nextVideoId: {nextVideoId}");
                    var findResult = _pyt.GetVideo(nextVideoId).Result;

                    var nextSong = new Song
                    {
                        VoiceChannel = first.VoiceChannel,
                        Title = findResult.Details.Title,
                        Artist = findResult.Details.Channel,
                        Url = $"https://youtu.be/{nextVideoId}",
                        StreamUrl = findResult.GetHighestQualityUrl(),
                        TimeQueued = DateTime.Now,
                        TextChannel = _recentTextChannel,
                    };

                    await Console.Out.WriteLineAsync($"AUTOSONG: {nextSong.Title}");
                    _playlist.QueueSong(nextSong);
                    return;
                }
                await Console.Out.WriteLineAsync("first was not from youtube");
            }

            await Console.Out.WriteLineAsync("NO SONG");
            return;
        }

        if (string.IsNullOrEmpty(song.StreamUrl))
        {
            await Console.Out.WriteLineAsync("AudioStreamUrl empty!!");
            return;
        }

        song.TimePlaying = DateTime.Now;

        _chatter.SendSongPlaying(song.TextChannel, song);

        _recentTextChannel = song.TextChannel;
        _ = _audioManager.PlayAudio(song, song.StartDuration);
    }

    private void Player_PlaybackFinished()
    {
        _playlist.Next();
    }

    public void Pause()
    {
        _audioManager.Pause();
    }

    public async Task ContinuePlay()
    {
        if (_songStore.Current != null)
        {
            _chatter.Send(_songStore.Current.TextChannel, ":arrow_forward: PLAY :arrow_forward:");
        }
        await _audioManager.ContinuePlay();
    }
}




//var ffmpegProcess = new Process
//{
//    StartInfo = new ProcessStartInfo
//    {
//        FileName = "ffmpeg",
//        Arguments = $"-reconnect_streamed 1 -i {audioUrl} -f s16le -ar 48000 -ac 2 pipe:1",
//        RedirectStandardOutput = true,
//        RedirectStandardError = true,
//        UseShellExecute = false,
//        CreateNoWindow = true,

//    }
//};

//ffmpegProcess.ErrorDataReceived += (s, e) =>
//{
//    Console.WriteLine($"error: {e.Data}");
//};

//ffmpegProcess.Start();
//ffmpegProcess.BeginErrorReadLine();

//var ffmpegOutputStream = ffmpegProcess.StandardOutput.BaseStream;
//var format = new WaveFormat(48000, 16, 2); // Opus format
//var bufferedStream = new BufferedWaveProvider(format);

//try
//{
//    var buffer = new byte[format.AverageBytesPerSecond / 2];

//    while (true)
//    {
//        int retries = 0;
//        if (_token.IsCancellationRequested)
//            break;
//        int bytesRead = 0;
//        try
//        {
//            bytesRead = ffmpegOutputStream.Read(buffer, 0, buffer.Length);
//            if (bytesRead == 0) break;
//        }
//        catch (Exception e)
//        {
//            retries++;
//            await Console.Out.WriteLineAsync($"Error failed read ({retries}): {e.Message}");
//            Thread.Sleep(200);
//            continue;
//        }

//        bufferedStream.AddSamples(buffer, 0, bytesRead);

//        while (bufferedStream.BufferedBytes >= format.AverageBytesPerSecond / 4)
//        {
//            int bytesToRead = bufferedStream.BufferedBytes;
//            bytesToRead -= bytesToRead % format.BlockAlign;
//            if (bytesToRead > 0)
//            {
//                byte[] outputBuffer = new byte[bytesToRead];
//                int bytesWritten = bufferedStream.Read(outputBuffer, 0, bytesToRead);
//                if (bytesWritten > 0)
//                {
//                    _discStream.Write(outputBuffer, 0, bytesWritten);
//                }
//            }
//        }
//    }
//}
//finally
//{
//    ffmpegProcess.Kill();
//}

//int channels = 2;
//int sampleRate = 48000;
//int bits = 16;



//  AI VOICE
//  AI VOICE
//  AI VOICE
//  AI VOICE

//var ai = new PoinkoAiVoice();
//audioUrl = await ai.GetAudioUrl("good morning sir yu lil shit");

//using var webClient = new WebClient();
//await Console.Out.WriteLineAsync("downloading data..");
//byte[] audioData = webClient.DownloadData(audioUrl);

//await Console.Out.WriteLineAsync("memory stream...");
//using var audioStream = new MemoryStream(audioData);

//await Console.Out.WriteLineAsync("mp3 reader...");
//using var reader = new Mp3FileReader(audioStream);

//await Console.Out.WriteLineAsync("copying (conversion)...");
//using var conversionStream = new WaveFormatConversionStream(new WaveFormat(sampleRate, bits, channels), reader);

//await Console.Out.WriteLineAsync("copying...");
//await conversionStream.CopyToAsync(_discStream, _token.Token);

//await Console.Out.WriteLineAsync("mp3 copy...");
//await reader.CopyToAsync(_discStream, _token.Token);

//return;


