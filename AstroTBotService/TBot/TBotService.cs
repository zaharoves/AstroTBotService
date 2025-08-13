using Telegram.Bot.Polling;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using AstroTBotService.Redis;
using System.Globalization;
using static Telegram.Bot.TelegramBotClient;

namespace AstroTBotService.TBot
{
    public class TBotService(
        IResourcesLocaleManager localeManager,
        ITelegramBotClient botClient,
        ILogger<TBotUpdateHandler> logger,
        IRedisService redisService,
        IHost host) : IHostedService
    {
        private readonly IResourcesLocaleManager _localeManager = localeManager;
        private readonly ITelegramBotClient _botClient = botClient;
        private readonly ILogger<TBotUpdateHandler> _logger = logger;
        private readonly IRedisService _redisService = redisService;
        private readonly IHost _host = host;

        private CancellationTokenSource _cts;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                //TODO
                _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = Array.Empty<UpdateType>()
                };

                _botClient.StartReceiving(
                    updateHandler: UpdateHandler,
                    errorHandler: ErrorHandler,
                    receiverOptions: receiverOptions,
                    cancellationToken: _cts.Token
                );

                _logger.LogInformation("Bot started receiving updates.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bot start receiving error.");
            }

            return;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogError("Bot is stopping.");

            _cts?.Cancel();
            _cts?.Dispose();

            return Task.CompletedTask;
        }

        private async Task UpdateHandler(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {
            var chatIdStr = update?.Message?.Chat?.Id.ToString()
                ?? update?.CallbackQuery?.Message?.Chat?.Id.ToString();

            if (update == null 
                || string.IsNullOrWhiteSpace(chatIdStr) 
                || !long.TryParse(chatIdStr, out var chatId))
            {
                throw new ArgumentException(chatIdStr);
            }

            var isForbidMessage = await _redisService.CheckMessageForbidTime(chatId);

            if (isForbidMessage)
            {
                var defaultLanguage = update?.CallbackQuery?.From?.LanguageCode
                    ?? update?.Message?.From?.LanguageCode
                    ?? "en";

                var cultureInfo = new CultureInfo(defaultLanguage);

                await _botClient.SendMessage(
                    chatId: chatId,
                    text: _localeManager.GetString("PleaseWait", cultureInfo),
                    replyMarkup: null);

                return;
            }
            else
            {
                await _redisService.SetMessageForbidTime(chatId);
            }

            var task = Task.Run(async () =>
                {
                    using (var scope = _host.Services.CreateScope())
                    using (_logger.BeginScope("user_chat_id: {user_chat_id}", chatIdStr))
                    {
                        try
                        {
                            var updateHandler = scope.ServiceProvider.GetRequiredService<IUpdateHandler>();
                            await updateHandler.HandleUpdateAsync(botClient, update, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Handle update error");
                        }
                        finally
                        {
                            try
                            {
                                await _redisService.DeleteMessageForbidTime(chatId);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Error deleting forbid time for chat {chatId}");
                            }
                        }
                    }
                },
                _cts.Token);

            await task;
        }

        private async Task ErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception.ToString();

            if (exception is ApiRequestException apiRequestException)
            {
                errorMessage = $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}";
            }

            _logger.LogError(exception, errorMessage);

            await Task.CompletedTask;
        }
    }
}
