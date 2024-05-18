using Ponko.DiscordBot.Common;

namespace Ponko.DiscordBot.Repositories;

public interface IGuildRepository
{
    IEnumerable<Guild> Items { get; }
    void InitGuilds(IEnumerable<Guild> guilds);
    void Add(Guild guild);
    void Remove(Guild guild);
    Guild Get(ulong id);
}