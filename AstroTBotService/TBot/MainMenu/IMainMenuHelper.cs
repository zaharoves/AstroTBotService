using AstroTBotService.Entities;
using Telegram.Bot.Types.ReplyMarkups;

namespace AstroTBotService.TBot
{
    public interface IMainMenuHelper
    {
        Task SendMainMenu(TBotClientData clientData);

        Task EditToMainMenu(TBotClientData clientData);

        Task SendLanguagePicker(TBotClientData clientData);

        Task SendMessage(long chatId, string message, ReplyMarkup replyMarkup);

        Task SendMessageHtml(long chatId, List<string> message, ReplyMarkup replyMarkup);

        InlineKeyboardButton GetCancelButton(TBotClientData clientData);

        InlineKeyboardButton GetCancelButton(TBotClientData clientData, string buttonText);
    }
}
