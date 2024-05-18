using Discord.WebSocket;
using Ponko.DiscordBot.Common;

namespace Ponko.DiscordBot.Commands
{
    struct SwayInt
    {
        private bool _increase;
        private int _count;
        private int _max;

        public SwayInt(int maxCount, int startCount)
        {
            _max = maxCount;
            _count = startCount;
            _increase = true;
        }
        public int NextCount()
        {
            if (_increase && _count >= _max)
                _increase = false;
            else if (!_increase && _count <= 0)
                _increase = true;
            _count += _increase ? 1 : -1;
            return _count;
        }
    }

    internal class CoinFlipCommand : IChatCommand
    {
        private readonly IChatter _chatter;

        public string Triggers => "coinflip,flipcoin,flip";

        public Guild Guild { get; set; }

        private bool isRolling = false;

        SwayInt _start;
        SwayInt _end;

        public CoinFlipCommand(IChatter chatter)
        {
            _chatter = chatter;
            _start = new(3, 3);
            _end = new(3, 0);
        }

        public async Task MessageReceived(SocketMessage msg, string trigger, string query)
        {
            var channel = msg.Channel as SocketTextChannel;

            if (channel == null)
                return;

            var response = await _chatter.Send(channel, "## FLIPPING");

            await Task.Run(async () =>
            {
                int count = 0;
                float maxCount = Random.Shared.Next(5, 8);
                while (count < maxCount)
                {
                    count++;
                    await Task.Delay(555);

                    string flippingText = "# ";
                    int startCount = _start.NextCount();
                    for (int i = 0; i < startCount; i++)
                    {
                        flippingText += " .";
                    }

                    flippingText += " FLIPPING ";

                    int endCount = _end.NextCount();
                    for (int i = 0; i < endCount; i++)
                    {
                        flippingText += ". ";
                    }
                    await response.ModifyAsync(msg => msg.Content = flippingText);
                }

                await Task.Delay(1000);

                int roll = Random.Shared.Next(0, 2);
                string flipResult = roll == 0 ? "# HEADS" : "# TAILS";
                await response.ModifyAsync(msg => msg.Content = flipResult);

            });
        }
    }
}
