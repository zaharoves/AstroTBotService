using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace AstroTBotService.TBot
{
    public interface IMainMenuHelper
    {
        Task SendMainMenu(ITelegramBotClient botClient, long chatId);

        Task SendMainMenu(ITelegramBotClient botClient, long chatId, int editMessageId);

        Task SendMessage(long chatId, string message, ReplyMarkup replyMarkup);
    }
}
