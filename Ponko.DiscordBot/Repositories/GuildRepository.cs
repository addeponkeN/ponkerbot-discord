using Ponko.DiscordBot.Models;
using System.Collections.Concurrent;

namespace Ponko.DiscordBot.Repositories;

public class GuildRepository : IGuildRepository
{
    private Dictionary<ulong, Guild> _guilds = new();
    
    public IEnumerable<Guild> Items => _guilds.Values;
    
    public void Add(Guild guild)
    {
        _guilds.TryAdd(guild.Id, guild);
    }

    public Guild? Get(ulong id)
    {
        _guilds.TryGetValue(id, out Guild guild);
        return guild;
    }

    public void InitGuilds(IEnumerable<Guild> guilds)
    {
        var guildPairs = new List<KeyValuePair<ulong, Guild>>();

        foreach (var guild in guilds)
        {
            guildPairs.Add(new KeyValuePair<ulong, Guild>(guild.Id, guild));
            Console.WriteLine($"joined server: {guild.Socket.Name} ({guild.Id})");
        }

        _guilds = new(guildPairs);
    }

    public void Remove(Guild guild)
    {
        //var kp = new KeyValuePair<ulong, Guild>(guild.Id, guild);
        _guilds.Remove(guild.Id);
    }
}