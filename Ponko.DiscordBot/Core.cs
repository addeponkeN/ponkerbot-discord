using Autofac;

namespace Ponko.DiscordBot;

public class Core
{
    public static IContainer Services { get; private set; }
    public static PonkoDiscord Discord { get; private set; }

    public static void Start()
    {
        Services = ServiceContainer.Configure();

        Discord = new PonkoDiscord();
        Discord.Start();

    }
}