using AstroTBotService.AstroCalculation.Entities;
using AstroTBotService.Entities;
using AstroTBotService.Enums;
using System.Globalization;
using Telegram.Bot.Types.ReplyMarkups;

namespace AstroTBotService.TBot
{
    public interface ITClientHelper
    {
        Task SendMainMenu(TBotClientData clientData);

        Task EditToMainMenu(TBotClientData clientData);



        Task SendLanguagePicker(TBotClientData clientData);

        Task SendHouseSystemPicker(TBotClientData clientData);


        Task SendMessage(long chatId, string message, ReplyMarkup replyMarkup);

        Task SendMessageHtml(long chatId, List<string> message, ReplyMarkup replyMarkup);

        InlineKeyboardButton GetCancelButtonWithEdit(TBotClientData clientData);

        InlineKeyboardButton GetCancelButton(TBotClientData clientData, string buttonText);

        List<string> GetChartMessages(List<AspectInfo> aspects, TBotClientData clientData, ChartTypeEnum chartTypeEnum);

        string GetNatalPlanetsMessage(ChartInfo chartInfo, TBotClientData clientData);

        string GetHousesMessage(ChartInfo chartInfo, TBotClientData clientData);
    }
}
