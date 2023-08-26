// See https://aka.ms/new-console-template for more information

using Ponko.DiscordBot;

internal class Program
{
    public static async Task Main(string[] args)
    {
        // Core.Start();
        //
        // Console.WriteLine("Hello, World!");
        //
        // Console.WriteLine($"main thread id: {Thread.CurrentThread.ManagedThreadId}");
        //
        // await Task.Run(async () =>
        // {
        //     await Task.Delay(50);
        //     Console.WriteLine($"task thread id: {Thread.CurrentThread.ManagedThreadId}");
        // });
        //
        // Console.ReadLine();

        PonkoDiscord client = new PonkoDiscord();
        await client.Start();
    }
}