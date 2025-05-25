using Telegram.Bot;

namespace AstroTBotService.TBot
{
    public interface IMainMenuHelper
    {
        Task SendMainMenu(ITelegramBotClient botClient, long chatId);

        Task SendMessage(long chatId, string message);
    }
}
