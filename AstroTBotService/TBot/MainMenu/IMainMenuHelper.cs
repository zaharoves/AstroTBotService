using Telegram.Bot;

namespace AstroTBotService.TBot
{
    public interface IMainMenuHelper
    {
        public Task SendMainMenu(ITelegramBotClient botClient, long chatId, bool isBirthdateExist);
    }
}
