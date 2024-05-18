using System.Reflection;
using Autofac;
using Ponko.DiscordBot.Common;
using Ponko.DiscordBot.Contracts;

namespace Ponko.DiscordBot;

public static class ServiceContainer
{
    public static IContainer Configure()
    {
        ContainerBuilder builder = new ContainerBuilder();

        builder.RegisterType<CommandHandler>().As<ICommandHandler>();

        var assembly = Assembly.GetExecutingAssembly();

        builder.RegisterAssemblyTypes(assembly)
            .Where(x => x.Namespace.Contains("Services"))
            .AsImplementedInterfaces();

        var container = builder.Build();

        var cm = container.Resolve<ICommandHandler>();

        return builder.Build();
    }
}