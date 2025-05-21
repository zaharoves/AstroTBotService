using Telegram.Bot.Polling;
using Telegram.Bot;

namespace AstroTBotService.TBot
{
    public class TBotService : BackgroundService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUpdateHandler _updateHandler;

        public TBotService(ITelegramBotClient botClient, IUpdateHandler updateHandler)
        {
            _botClient = botClient;
            _updateHandler = updateHandler;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var botName = _botClient.GetMe().Result.FirstName;

                var cts = new CancellationTokenSource();
                var cancellationToken = cts.Token;

                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = { }, // receive all update types
                };

                _botClient.StartReceiving(
                    _updateHandler.HandleUpdateAsync,
                    _updateHandler.HandleErrorAsync,
                    receiverOptions,
                    cancellationToken
                );

                Console.WriteLine($"Запущен бот {botName}");

                var me = await _botClient.GetMe(cancellationToken);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\"Произошла непредвиденная ошибка в Telegram Bot Worker.\"");
            }
            finally
            {
                Console.WriteLine("Telegram Bot Worker остановлен.");
            }
        }
    }
}
