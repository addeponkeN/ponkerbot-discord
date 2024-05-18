using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ponko.DiscordBot.Commands;
using Ponko.DiscordBot.Common;
using Ponko.MediaLib;
using Ponko.MusicFinder;
using Ponko.YT;
using System.Reflection;

namespace Ponko.DiscordBot;

public class GuildService<T> where T : class
{
    public T this[Guild guild] => Get(guild);
    public T Get(Guild guild)
    {
        var svc = PonkoDiscordHost._guildServices.Get<T>(guild);
        return svc;
    }
}

public class CommandHost
{
    public Type CommandType { get; set; }
    public HashSet<string> Triggers { get; set; }
}

public class CommandFactory
{
    private readonly Dictionary<Guild, CommandHost> _commands = new();
    private readonly List<CommandHost> _hosts = new();

    public void Register<TImpl>(IEnumerable<string> triggers) where TImpl : class
    {
        _hosts.Add(new CommandHost { CommandType = typeof(TImpl), Triggers = triggers.ToHashSet() });
    }

    public IChatCommand GetCommand(Guild guild, string trigger)
    {
        for (int i = 0; i < _hosts.Count; i++)
        {
            var h = _hosts[i];
            foreach (var t in h.Triggers)
            {
                if (h.Triggers.Contains(trigger))
                {
                    var cmd = PonkoDiscord.AppHost.Services.GetRequiredService(h.CommandType) as IChatCommand;
                    cmd.Guild = guild;
                    return cmd;
                }
            }
        }
        return null;
    }
}

public class GuildInfo
{
    public Guild Guild;
}

public class GuildServices
{
    private readonly HashSet<Type> _registered = new();
    private readonly Dictionary<ulong, Dictionary<Type, object>> _services = new();

    public T Get<T>(Guild guild)
        where T : class
    {
        var t = typeof(T);
        ulong id = guild.Id;

        if (!_registered.Contains(t))
        {
            throw new Exception("Service not implemented");
        }

        if (!_services.TryGetValue(id, out var types))
        {
            _services.Add(id, types = new());
        }

        T retVal;

        if (!types.TryGetValue(t, out var outVal))
        {
            retVal = PonkoDiscord.AppHost.Services.GetService<T>();
            if (retVal is IGuildService guildSvc)
            {
                guildSvc.OnCreate(guild);
            }
            types.Add(t, retVal);
        }
        else
        {
            retVal = (outVal as T)!;
        }

        return retVal!;
    }

    internal void Register<TImpl>() where TImpl : class
    {
        _registered.Add(typeof(TImpl)); ;
    }
}

public class PonkoDiscord
{
    public static IHost AppHost { get; private set; }

    public async Task Start()
    {
        Console.WriteLine("starting");

        AppHost = SetupApp();

        AppHost.Services.GetRequiredService<PonkoDiscordHost>();

        await AppHost.RunAsync();
        await Task.Delay(-1);
    }

    private IHost SetupApp(HostApplicationBuilder builder = null!)
    {
        builder ??= Host.CreateApplicationBuilder();

        ConfigureServices(builder.Services);

        var app = builder.Build();

        return app;
    }

    private void ConfigureServices(IServiceCollection services)
    {
        //  main
        services.AddSingleton<PonkoDiscordHost>();
        services.AddSingleton<IDiscordTokenStore, DiscordTokenStore>();

        //  util
        services.AddTransient<ILogger, DefaultLogger>();

        // misc
        services.AddTransient<IGuildPlaylistRepository, GuildPlaylist>();

        //  commands
        services.AddSingleton<CommandHandler>();
        services.AddTransient<ICommandProvider, CommandProvider>();
        services.AddCommands();

        //  audio
        services.AddSingleton<IAudioClientManager, AudioClientManager>();
        services.AddSingleton<AudioManager>();
        services.AddSingleton<PonkoStreamPlayer>();

        //  chat
        services.AddTransient<IChatter, Chatter>();

        //  music
        services.AddSingleton<MusicManager>();
        services.AddSingleton<MediaPlaylist<Song>>();
        services.AddSingleton<SongStore>();
        services.AddSingleton<IMediaSongStore<Song>, SongStore>(p => p.GetRequiredService<SongStore>());
        services.AddSingleton<PonkoYT>();
        services.AddSingleton<IPonkoMusicFinder, PonkoMusicFinder>();
    }
}

public static class DiscordServiceExtensions
{
    public static IServiceCollection AddCommands(this IServiceCollection services)
    {
        var commandIfc = typeof(IChatCommand);
        var assembly = Assembly.GetExecutingAssembly();

        var implClasses = assembly.GetTypes()
            .Where(x => commandIfc.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);

        foreach (var chatCommand in implClasses)
        {
            services.AddTransient(chatCommand);
        }

        Console.WriteLine($"registered '{implClasses.Count()}' chat commands");

        return services;
    }

    public static IChatCommand GetCommand<T>(this IServiceProvider provider, Guild guild, string query)
    {
        return PonkoDiscordHost._commandFactory.GetCommand(guild, query);
    }

    public static T GetGuildService<T>(this IServiceProvider provider, Guild guild)
        where T : class
    {
        return PonkoDiscordHost._guildServices.Get<T>(guild);
    }

    public static IServiceCollection AddGuildService<TImpl>(this IServiceCollection services)
        where TImpl : class
    {
        PonkoDiscordHost._guildServices.Register<TImpl>();
        services.AddTransient<TImpl>();
        services.AddTransient<GuildService<TImpl>>();
        return services;
    }

    public static IServiceCollection AddGuildService<TService, TImpl>(this IServiceCollection services)
        where TService : class
        where TImpl : class, TService
    {
        PonkoDiscordHost._guildServices.Register<TImpl>();
        services.AddTransient<TService, TImpl>();
        services.AddTransient<GuildService<TImpl>>();
        return services;
    }

    public static IServiceCollection AddCommand<TImpl>(this IServiceCollection services, IEnumerable<string> triggers)
        where TImpl : class
    {
        PonkoDiscordHost._commandFactory.Register<TImpl>(triggers);
        services.AddTransient<TImpl>();
        return services;
    }

    /// <summary>
    /// Triggers are separated with , (comma).
    /// </summary>
    /// <param name="triggers">Triggers are separated with , (comma).</param>
    /// <returns></returns>
    public static IServiceCollection AddCommand<TCommand>(this IServiceCollection services, string triggers)
        where TCommand : class, IChatCommand
    {
        var splitTriggers = triggers.Trim().Split(',');
        return AddCommand<TCommand>(services, splitTriggers);
    }
}