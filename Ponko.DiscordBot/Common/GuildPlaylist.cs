using Ponko.DiscordBot.Commands;
using Ponko.Util;

namespace Ponko.DiscordBot.Common;

public class GuildPlaylistRepositoryFile
{
    public GuildPlaylistFile Playlist { get; set; }
}

public class GuildPlaylistFile
{
    public string Name { get; set; }
    public SongFile[] Songs { get; set; }
}

public class SongFile
{
    public string Title { get; set; }
    public string VideoUrl { get; set; }
}

public interface IGuildPlaylistRepository
{
    IEnumerable<SongFile> Playlist { get; }
    bool Add(SongFile songFile);
    bool Delete(SongFile songFile);
}

public class GuildPlaylist : IGuildPlaylistRepository
{
    private string RootPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private const string FileName = "playlist";

    private Guild _guild;
    private List<SongFile> _songs;

    public IEnumerable<SongFile> Playlist => _songs;

    public GuildPlaylist()
    {
        _songs = new();
    }

    public void SetGuild(Guild guild)
    {
        _guild = guild;
    }

    private string GetFinalPath()
    {
        string dataFolderName = "crookbot\\playlist";
        string fileName = $"{FileName}_{_guild.Id}";
        string path = Path.Combine(RootPath, dataFolderName, fileName);
        return path;
    }

    private async Task LoadPlaylist()
    {
        string path = GetFinalPath();

        var file = await JsonHelper.Load<GuildPlaylistRepositoryFile>(path);

        if (file != null)
        {
            _songs = file.Playlist.Songs.ToList();
        }
    }

    private void SavePlaylist()
    {
        string path = GetFinalPath();

        var playlist = new GuildPlaylistFile
        {
            Songs = Playlist.ToArray(),
        };

        _ = JsonHelper.Save(path, playlist);
    }

    public bool Delete(SongFile songFile)
    {
        if (_songs.Remove(songFile))
        {
            SavePlaylist();
            return true;
        }

        return false;
    }

    public bool Add(SongFile songFile)
    {
        if(_songs.Any(x=>x.Title == songFile.Title))
        {
            return false;
        }

        _songs.Add(songFile);

        return true;
    }
}
