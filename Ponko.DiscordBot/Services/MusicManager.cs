using Discord.Audio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Ponko.DiscordBot.Commands;
using Ponko.DiscordBot.Models;
using Ponko.MediaLib;
using Ponko.YT;

namespace Ponko.DiscordBot.Services;

public class MusicManager
{
    private readonly Guild _guild;
    private readonly PonkoStreamPlayer _player;
    private readonly MediaPlaylist<Song> _playlist;
    private readonly SongStore _songStore;
    private readonly YT.PonkoYT _yt;

    AudioOutStream _discStream;
    MediaFoundationReader _stream;

    CancellationTokenSource _token;

    public MusicManager(Guild guild, PonkoStreamPlayer player, MediaPlaylist<Song> playlist, SongStore songStore, YT.PonkoYT yt)
    {
        _guild = guild;
        _player = player;
        _playlist = playlist;
        _songStore = songStore;
        _yt = yt;

        _songStore.SongChangedEvent += SongStore_SongChangedEvent;
        _player.PlaybackFinished += Player_PlaybackFinished;

    }

    public void StopStream()
    {
        if (_token != null)
        {
            _token.Cancel();
            Console.WriteLine("cancelled");
        }

        if (_stream != null)
        {
            _player.ClearStream();
            _stream = null;
        }

    }

    private async void SongStore_SongChangedEvent(Song? song)
    {
        if (song == null)
        {
            await Console.Out.WriteLineAsync("NO SONG");
            StopStream();
            return;
        }

        if (string.IsNullOrEmpty(song.AudioStreamUrl))
        {
            await Console.Out.WriteLineAsync("AudioStreamUrl empty!!");
            return;
        }

        StopStream();

        _token = new CancellationTokenSource();

        if (!_guild.IsVoiceConnected)
        {
            await _guild.ConnectVoice(song.VoiceChannel);
        }

        _discStream ??= _guild.AudioClient.CreatePCMStream(AudioApplication.Music);

        _stream = await _player.CreateStream(song.AudioStreamUrl);

        int retries = 10;

        while (_stream == null)
        {
            if (retries < 0)
            {

                return;
            }

            Thread.Sleep(500);
            _stream = await _player.CreateStream(song.AudioStreamUrl);
            await Console.Out.WriteLineAsync("getting stream...");
            retries--;
        }

        await Console.Out.WriteLineAsync("streaming!");

        try
        {
            int channels = 2;
            int sampleRate = 48000;
            int bits = 16;
            if (_stream.WaveFormat.Channels != channels ||
                _stream.WaveFormat.SampleRate != sampleRate ||
                _stream.WaveFormat.BitsPerSample != bits)
            {
                var conversionStream = new WaveFormatConversionStream(new WaveFormat(sampleRate, bits, channels), _stream);
                await conversionStream.CopyToAsync(_discStream, _token.Token);
            }
            else
            {
                await _stream.CopyToAsync(_discStream, _token.Token);
            }
        }
        catch
        {
            //await Console.Out.WriteLineAsync($"CopyToAsync done !!: {e.Message}");
        }


    }

    private void Player_PlaybackFinished()
    {
        _playlist.Next();
    }

    private void Player_OnPositionChanged(int obj)
    {

    }

}
