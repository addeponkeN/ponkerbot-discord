namespace Ponko.DiscordBot;

public interface IDiscordTokenStore
{
    string Token { get; }
}

public class DiscordTokenStore : IDiscordTokenStore
{
    private string _token;
    private Task<string> _loadTask;

    public string Token => _token ??= GetToken();

    public DiscordTokenStore()
    {
        _loadTask = LoadToken();
    }

    private async Task<string> LoadToken()
    {
        string appRoot = Directory.GetCurrentDirectory();
        const string tokenFilename = "discordtoken.txt";

        string tokenPath = Path.Combine(appRoot, tokenFilename);
        if (!File.Exists(tokenPath))
        {
            Console.WriteLine("sorry sir/mam, no token found!! ('discordtoken.txt' containing the discord token)");
            Console.ReadLine();
            return string.Empty;
        }

        return await File.ReadAllTextAsync(tokenPath);
    }

    private string GetToken()
    {
        return _loadTask.Result;
    }

}