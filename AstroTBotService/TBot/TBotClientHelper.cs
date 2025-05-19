using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace AstroTBotService.TBot
{
    public class TBotClientHelper : ITBotClientHelper
    {
        private readonly ITelegramBotClient _botClient;

        public TBotClientHelper(ITelegramBotClient botClient) 
        {
            _botClient = botClient;
        }

        public async Task SendMessageAsync(string rmqMessageId, string messageText)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new [] { InlineKeyboardButton.WithCallbackData("Рассчитать новую дату", $"start:") }
            });

            if (TBotHandler.RmqDict.TryGetValue(rmqMessageId, out var chatId))
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: messageText,
                    replyMarkup: inlineKeyboard);
            }
        }

    }
}
