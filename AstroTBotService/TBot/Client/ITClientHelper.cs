using AstroCalculation.Entities;
using AstroCalculation.Enums;
using AstroTBotService.Db.Entities;
using AstroTBotService.Entities;
using System.Globalization;
using Telegram.Bot.Types.ReplyMarkups;

namespace AstroTBotService.TBot
{
    public interface ITClientHelper
    {
        Task SendMenu(TBotClientData clientData);

        Task EditToMenu(TBotClientData clientData);



        Task SendLanguagePicker(TBotClientData clientData);

        Task SendHouseSystemPicker(TBotClientData clientData);

        Task SendPersons(TBotClientData clientData, string commandButtonPrefix);
        Task SendEditPersons(TBotClientData clientData);


        Task SendMessage(long chatId, string message, ReplyMarkup replyMarkup);

        Task SendMessageHtml(long chatId, List<string> message, ReplyMarkup replyMarkup);
        Task EditMessage(long chatId, int messageId, string messageText, InlineKeyboardMarkup replyMarkup);



        InlineKeyboardButton GetCancelButtonWithEdit(TBotClientData clientData);

        InlineKeyboardButton GetCancelButton(TBotClientData clientData, string buttonText);

        InlineKeyboardButton GetBackToPersonsButton(TBotClientData clientData);



        List<string> GetChartMessages(List<AspectInfo> aspects, IAstroPerson astroPerson, CultureInfo cultureInfo, ChartTypeEnum chartTypeEnum);

        string GetNatalPlanetsMessage(ChartInfo chartInfo, IAstroPerson astroPerson, CultureInfo cultureInfo);

        string GetHousesMessage(ChartInfo chartInfo, IAstroPerson astroPerson, CultureInfo cultureInfo);
    }
}
