using AstroTBotService.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace AstroTBotService.TBot
{
    public interface IDatePicker
    {
        bool TryParseDateTimePicker(CallbackQuery callbackQuery, out DatePickerData datePickerData);

        Task SendYearIntervalPicker(ITelegramBotClient botClient, CallbackQuery callbackQuery, string text);

        Task SendYearPicker(ITelegramBotClient botClient, CallbackQuery callbackQuery, DatePickerData datePickerData, string text);

        Task SendMonthPicker(ITelegramBotClient botClient, CallbackQuery callbackQuery, DatePickerData datePickerData, string text);

        Task SendDayPicker(ITelegramBotClient botClient, CallbackQuery callbackQuery, DatePickerData datePickerData, string text);

        Task SendHourPicker(ITelegramBotClient botClient, CallbackQuery callbackQuery, DatePickerData datePickerData, string text);

        Task SendMinutePicker(ITelegramBotClient botClient, CallbackQuery callbackQuery, DatePickerData datePickerData, string text);

        Task SendTimeZonePicker(ITelegramBotClient botClient, CallbackQuery callbackQuery, DatePickerData datePickerData, string text);

        Task SendConfirmDate(ITelegramBotClient botClient, CallbackQuery callbackQuery, DatePickerData datePickerData, string text);
    }
}
