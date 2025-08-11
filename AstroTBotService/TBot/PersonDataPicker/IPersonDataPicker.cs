using AstroTBotService.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace AstroTBotService.TBot
{
    public interface IPersonDataPicker
    {
        Task SendYearIntervalPicker(TBotClientData clientData, string text);
        Task EditToYearIntervalPicker(TBotClientData clientData, string text);

        Task SendYearPicker(TBotClientData clientData, string text);

        Task SendMonthPicker(TBotClientData clientData, string text);

        Task SendDayPicker(TBotClientData clientData, string text);

        Task SendHourPicker(TBotClientData clientData, string text);

        Task SendMinutePicker(TBotClientData clientData, string text);

        Task SendTimeZonePicker(TBotClientData clientData, string text);

        Task SendConfirmDate(TBotClientData clientData, string text);
    }
}
