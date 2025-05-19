using System.Globalization;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using AstroTBotService.Constans;

namespace AstroTBotService.TBot
{
    public class TBotDatePicker : ITBotDatePicker
    {
        private InlineKeyboardMarkup GetCalendarMonthKeyboard(int year)
        {
            var dateTimePicker = new List<List<InlineKeyboardButton>>();

            DateTimeFormatInfo dtFormatInfo = CultureInfo.CurrentCulture.DateTimeFormat;

            //4 Rows
            for (var rowNum = 0; rowNum < 4; rowNum++)
            {
                var row = new List<InlineKeyboardButton>();

                //3 Months per row
                for (var columnNum = 0; columnNum < 3; columnNum++)
                {
                    var monthNum = rowNum * 3 + columnNum + 1;

                    row.Add(InlineKeyboardButton.WithCallbackData($"{dtFormatInfo.GetAbbreviatedMonthName(monthNum)}", $"dateTimePicker:{year}:{monthNum}"));
                }

                dateTimePicker.Add(row);
            }

            return new InlineKeyboardMarkup(dateTimePicker);
        }

        public InlineKeyboardMarkup GetCalendarDaysKeyboard(int year, int month)
        {
            var dateTimePicker = new List<List<InlineKeyboardButton>>();

            var daysInMonth = DateTime.DaysInMonth(year, month);
            var currentDay = 1;

            //4 or 5 Rows
            for (var rowNum = 0; rowNum < 5; rowNum++)
            {
                var row = new List<InlineKeyboardButton>();

                if (currentDay > daysInMonth)
                {
                    break;
                }

                //7 Days per row
                for (var columnNum = 0; columnNum < 7; columnNum++)
                {
                    if (currentDay > daysInMonth)
                    {
                        row.Add(InlineKeyboardButton.WithCallbackData(" ", $"ignore"));
                        continue;
                    }

                    row.Add(InlineKeyboardButton.WithCallbackData($"{currentDay}", $"dateTimePicker:{year}:{month}:{currentDay}"));

                    currentDay++;
                }

                dateTimePicker.Add(row);
            }

            return new InlineKeyboardMarkup(dateTimePicker);
        }

        public InlineKeyboardMarkup GetCalendarHourKeyboard(int year, int month, int day)
        {
            var dateTimePicker = new List<List<InlineKeyboardButton>>();

            var currentHour = 0;

            //4 Rows
            for (var rowNum = 0; rowNum < 4; rowNum++)
            {
                var row = new List<InlineKeyboardButton>();

                //6 Cells per row
                for (var columnNum = 0; columnNum < 6; columnNum++)
                {
                    row.Add(InlineKeyboardButton.WithCallbackData($"{currentHour}", $"dateTimePicker:{year}:{month}:{day}:{currentHour}"));

                    currentHour++;
                }

                dateTimePicker.Add(row);
            }

            return new InlineKeyboardMarkup(dateTimePicker);
        }

        public InlineKeyboardMarkup GetCalendarMinuteKeyboard(int year, int month, int day, int hour)
        {
            var dateTimePicker = new List<List<InlineKeyboardButton>>();

            var currentMinute = 0;

            //6 Rows
            for (var rowNum = 0; rowNum < 10; rowNum++)
            {
                var row = new List<InlineKeyboardButton>();

                //10 Cells per row
                for (var columnNum = 0; columnNum < 6; columnNum++)
                {
                    row.Add(InlineKeyboardButton.WithCallbackData($"{currentMinute}", $"dateTimePicker:{year}:{month}:{day}:{hour}:{currentMinute}"));

                    currentMinute++;
                }

                dateTimePicker.Add(row);
            }

            return new InlineKeyboardMarkup(dateTimePicker);
        }

        public InlineKeyboardMarkup GetGmtKeyboard(int year, int month, int day, int hour, int minute)
        {
            var dateTimePicker = new List<List<InlineKeyboardButton>>();

            //24 Rows
            for (var rowNum = 0; rowNum < 24; rowNum++)
            {
                var row = new List<InlineKeyboardButton>();

                var sign = TBotClientConstants.TimeZoneDict[rowNum].TimeZoneInt >= 0 ? "+" : "-";
                var buttonText = $"[GMT{sign}{Math.Abs(TBotClientConstants.TimeZoneDict[rowNum].TimeZoneInt)}] ({TBotClientConstants.TimeZoneDict[rowNum].Description})";

                row.Add(InlineKeyboardButton.WithCallbackData(
                    buttonText,
                    $"dateTimePicker:{year}:{month}:{day}:{hour}:{minute}:{TBotClientConstants.TimeZoneDict[rowNum].TimeZoneInt}"));

                dateTimePicker.Add(row);
            }

            return new InlineKeyboardMarkup(dateTimePicker);
        }

        public async Task SendYearPicker(ITelegramBotClient botClient, long chatId, int year, string text)
        {
            var forceReplyMarkup = new ForceReplyMarkup() { Selective = true };

            await botClient.SendMessage(
                chatId: chatId,
                text: text,//$"Некорректное значение.\nВведите год Вашего рождения (от 1900 до {DateTime.Now.Year}):"
                replyMarkup: forceReplyMarkup);
        }

        public async Task SendMonthPicker(ITelegramBotClient botClient, long chatId, int year, string text)
        {
            var keyboard = GetCalendarMonthKeyboard(year);

            await botClient.SendMessage(
                chatId: chatId,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendDayPicker(ITelegramBotClient botClient, CallbackQuery callbackQuery, int year, int month, string text)
        {
            var keyboard = GetCalendarDaysKeyboard(year, month);

            long chatId = callbackQuery.Message.Chat.Id;
            int messageId = callbackQuery.Message.MessageId;

            await botClient.EditMessageText( //EditMessageReplyMarkup??
                chatId: chatId,
                messageId: messageId,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendHourPicker(ITelegramBotClient botClient, CallbackQuery callbackQuery, int year, int month, int day, string text)
        {
            var keyboard = GetCalendarHourKeyboard(year, month, day);

            long chatId = callbackQuery.Message.Chat.Id;
            int messageId = callbackQuery.Message.MessageId;

            await botClient.EditMessageText(
                chatId: chatId,
                messageId: messageId,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendMinutePicker(ITelegramBotClient botClient, CallbackQuery callbackQuery, int year, int month, int day, int hour, string text)
        {
            var keyboard = GetCalendarMinuteKeyboard(year, month, day, hour);

            long chatId = callbackQuery.Message.Chat.Id;
            int messageId = callbackQuery.Message.MessageId;

            await botClient.EditMessageText(
                chatId: chatId,
                messageId: messageId,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendGmtPicker(ITelegramBotClient botClient, CallbackQuery callbackQuery, int year, int month, int day, int hour, int minute, string text)
        {
            var keyboard = GetGmtKeyboard(year, month, day, hour, minute);

            long chatId = callbackQuery.Message.Chat.Id;
            int messageId = callbackQuery.Message.MessageId;

            await botClient.EditMessageText(
                chatId: chatId,
                messageId: messageId,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendConfirmDate(ITelegramBotClient botClient, CallbackQuery callbackQuery, int year, int month, int day, int hour, int minute, int gmtOffset, string text)
        {
            var birthTimeUtc = new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Utc);
            var timeSpanOffset = new TimeSpan(0, gmtOffset, 0, 0);


            //DateTimeOffset dateTimeOffset = new DateTimeOffset(birthTimeUtc, timeSpanOffset);

            long chatId = callbackQuery.Message.Chat.Id;
            int messageId = callbackQuery.Message.MessageId;

            // Создаем список рядов кнопок
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Рассчитать", $"process:"),
                    InlineKeyboardButton.WithCallbackData("Изменить дату рождения", $"start:")
                }
            });

            await botClient.EditMessageText(
                chatId: chatId,
                messageId: messageId,
                text: text,
                replyMarkup: inlineKeyboard);
        }

        public bool TryParseDateTimePicker(CallbackQuery callbackQuery, out int year, out int month, out int day, out int hour, out int minute, out int gmtOffset)
        {
            gmtOffset = 0;
            hour = 0;
            minute = 0;

            day = 0;
            month = 0;
            year = 0;

            if (callbackQuery.Data == null)
            {
                return false;
            }

            var parts = callbackQuery.Data.Split(':');

            gmtOffset = parts.Length > 6 && int.TryParse(parts[6], out var _gmtOffset) ? _gmtOffset : 0;
            minute = parts.Length > 5 && int.TryParse(parts[5], out var _minute) ? _minute : 0;
            hour = parts.Length > 4 && int.TryParse(parts[4], out var _hour) ? _hour : 0;
            day = parts.Length > 3 && int.TryParse(parts[3], out var _day) ? _day : 0;
            month = parts.Length > 2 && int.TryParse(parts[2], out var _month) ? _month : 0;
            year = parts.Length > 1 && int.TryParse(parts[1], out var _year) ? _year : 0;

            return true;
        }
    }
}
