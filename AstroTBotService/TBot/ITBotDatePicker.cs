using Telegram.Bot;
using Telegram.Bot.Types;

namespace AstroTBotService.TBot
{
    public interface ITBotDatePicker
    {
        bool TryParseDateTimePicker(CallbackQuery callbackQuery, out int year, out int month, out int day, out int hour, out int minute, out int gmtOffset);

        Task SendYearPicker(ITelegramBotClient botClient, long chatId, int year, string text);

        Task SendMonthPicker(ITelegramBotClient botClient, long chatId, int year, string text);

        Task SendDayPicker(ITelegramBotClient botClient, CallbackQuery callbackQuery, int year, int month, string text);

        Task SendHourPicker(ITelegramBotClient botClient, CallbackQuery callbackQuery, int year, int month, int day, string text);

        Task SendMinutePicker(ITelegramBotClient botClient, CallbackQuery callbackQuery, int year, int month, int day, int hour, string text);

        Task SendGmtPicker(ITelegramBotClient botClient, CallbackQuery callbackQuery, int year, int month, int day, int hour, int minute, string text);

        Task SendConfirmDate(ITelegramBotClient botClient, CallbackQuery callbackQuery, int year, int month, int day, int hour, int minute, int gmtOffset, string text);
    }
}
