using Ponko.AiVoice;
using Ponko.DiscordBot;
using Ponko.Udio;
using Ponko.YT;

internal class Program
{
    public static async Task Main(string[] args)
    {
        Console.Title = "CROOKBOT";
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

        //var pyt = new PonkoYT();
        //var a = pyt.GetRelatedVideo("4xPQ16Asyoo").Result;

        //var u = new UdioClient();
        //var info = await u.GetSongInfo("");
        //return;


        PonkoDiscord client = new PonkoDiscord();
        await client.Start();
    }
}