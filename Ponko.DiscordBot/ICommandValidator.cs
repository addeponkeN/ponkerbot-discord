namespace Ponko.DiscordBot;

public interface ICommandValidator
{
    bool IsValidCommand(string command);
}

public class CommandValidator : ICommandValidator
{
    private string _triggerPrefix = ".";

    public bool IsValidCommand(string command)
    {
        bool isNull = string.IsNullOrEmpty(command);
        string firstChar = command[0].ToString();
        bool isCommandPrefix = firstChar == _triggerPrefix;
        return !isNull && isCommandPrefix;
    }
}
