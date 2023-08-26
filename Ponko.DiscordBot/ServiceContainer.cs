using System.Reflection;
using Autofac;
using Ponko.DiscordBot.Contracts;
using Ponko.DiscordBot.Services;

namespace Ponko.DiscordBot;

public static class ServiceContainer
{
    public static IContainer Configure()
    {
        ContainerBuilder builder = new ContainerBuilder();

        builder.RegisterType<CommandManager>().As<ICommandManager>();

        var assembly = Assembly.GetExecutingAssembly();

        builder.RegisterAssemblyTypes(assembly)
            .Where(x => x.Namespace.Contains("Services"))
            .AsImplementedInterfaces();

        var container = builder.Build();

        var cm = container.Resolve<ICommandManager>();

        return builder.Build();
    }
}