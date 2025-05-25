using System.Globalization;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using AstroTBotService.Entities;

namespace AstroTBotService.TBot
{
    public class DatePicker : IDatePicker
    {
        public bool TryParseDateTimePicker(CallbackQuery callbackQuery, out DatePickerData datePickerData)
        {
            datePickerData = new DatePickerData();

            try
            {
                if (callbackQuery.Data == null)
                {
                    return false;
                }

                var parts = callbackQuery.Data.Split(':');

                var isSaveCommand = parts.Length > 8 && parts[8] == Constants.ButtonCommands.SAVE_BIRTHDAY;
                var isChangeCommand = parts.Length > 8 && parts[8] == Constants.ButtonCommands.CHANGE_BIRTHDAY;
                var isCancelCommand = parts.Length > 8 && parts[8] == Constants.ButtonCommands.TO_MAIN_MENU;
                var gmtOffset = parts.Length > 7 && int.TryParse(parts[7], out var _gmtOffset) ? _gmtOffset : 0;
                var minute = parts.Length > 6 && int.TryParse(parts[6], out var _minute) ? _minute : 0;
                var hour = parts.Length > 5 && int.TryParse(parts[5], out var _hour) ? _hour : 1;
                var day = parts.Length > 4 && int.TryParse(parts[4], out var _day) ? _day : 1;
                var month = parts.Length > 3 && int.TryParse(parts[3], out var _month) ? _month : 1;
                var year = parts.Length > 2 && int.TryParse(parts[2], out var _year) ? _year : 1;
                var yearInterval = parts.Length > 1 && int.TryParse(parts[1], out var _yearInterval) ? _yearInterval : 1;

                var dateTime = new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Unspecified);
                var timeSpan = new TimeSpan(0, gmtOffset, 0, 0);

                datePickerData.MinYearInterval = yearInterval;
                datePickerData.DateTime = dateTime;
                datePickerData.GmtOffset = timeSpan;

                datePickerData.IsSaveCommand = isSaveCommand;
                datePickerData.IsChangeCommand = isChangeCommand;
                datePickerData.IsCancelCommand = isCancelCommand;
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public async Task SendYearIntervalPicker(ITelegramBotClient botClient, CallbackQuery callbackQuery, string text)
        {
            var keyboard = GetYearIntervalKeyboard();

            await botClient.SendMessage(
                chatId: callbackQuery.Message.Chat.Id,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendYearPicker(ITelegramBotClient botClient, CallbackQuery callbackQuery, DatePickerData datePickerData, string text)
        {
            var keyboard = GetYearKeyboard(datePickerData);

            await botClient.EditMessageText(
                chatId: callbackQuery.Message.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendMonthPicker(ITelegramBotClient botClient, CallbackQuery callbackQuery, DatePickerData datePickerData, string text)
        {
            var keyboard = GetMonthKeyboard(datePickerData);

            await botClient.EditMessageText(
                chatId: callbackQuery.Message.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendDayPicker(ITelegramBotClient botClient, CallbackQuery callbackQuery, DatePickerData datePickerData, string text)
        {
            var keyboard = GetDaysKeyboard(datePickerData);

            await botClient.EditMessageText( //EditMessageReplyMarkup??
                chatId: callbackQuery.Message.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendHourPicker(ITelegramBotClient botClient, CallbackQuery callbackQuery, DatePickerData datePickerData, string text)
        {
            var keyboard = GetHourKeyboard(datePickerData);

            await botClient.EditMessageText(
                chatId: callbackQuery.Message.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendMinutePicker(ITelegramBotClient botClient, CallbackQuery callbackQuery, DatePickerData datePickerData, string text)
        {
            var keyboard = GetMinuteKeyboard(datePickerData);

            await botClient.EditMessageText(
                chatId: callbackQuery.Message.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendTimeZonePicker(ITelegramBotClient botClient, CallbackQuery callbackQuery, DatePickerData datePickerData, string text)
        {
            var keyboard = GetGmtKeyboard(datePickerData);

            await botClient.EditMessageText(
                chatId: callbackQuery.Message.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendConfirmDate(ITelegramBotClient botClient, CallbackQuery callbackQuery, DatePickerData datePickerData, string text)
        {
            var prefixCommand = $"{Constants.ButtonCommands.DATE_PICKER}:{datePickerData?.MinYearInterval}:{datePickerData?.DateTime?.Year}:{datePickerData?.DateTime?.Month}:{datePickerData?.DateTime?.Day}:{datePickerData?.DateTime?.Hour}:{datePickerData?.DateTime?.Minute}:{datePickerData?.GmtOffset.Hours}";

            // Создаем список рядов кнопок
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData($"Сохранить {Constants.Icons.Common.GREEN_CIRCLE}", $"{prefixCommand}:{Constants.ButtonCommands.SAVE_BIRTHDAY}"),
                    InlineKeyboardButton.WithCallbackData($"Изменить {Constants.Icons.Common.YELLOW_CIRCLE}", $"{prefixCommand}:{Constants.ButtonCommands.CHANGE_BIRTHDAY}"),
                },
                new []
                {
                    MainMenuHelper.GetCancelButton()
                }
            });

            await botClient.EditMessageText(
                chatId: callbackQuery.Message.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: text,
                replyMarkup: inlineKeyboard);
        }

        private InlineKeyboardMarkup GetYearIntervalKeyboard()
        {
            var dateTimePicker = new List<List<InlineKeyboardButton>>();

            var currentYear = DateTime.Now.Year;
            var intervalsCount = ((currentYear - Constants.START_INTERVAL_YEAR) / Constants.YEARS_INTERVAL) + 1;

            //10 years per row
            for (var rowNum = 0; rowNum < intervalsCount; rowNum++)
            {
                var startInterval = $"{Constants.START_INTERVAL_YEAR + (Constants.YEARS_INTERVAL * rowNum)}";
                var endInterval = rowNum == intervalsCount - 1
                    ? currentYear.ToString()
                    : $"{Constants.START_INTERVAL_YEAR + (Constants.YEARS_INTERVAL * (rowNum + 1)) - 1}";

                var row = new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData(
                    $"{startInterval} - {endInterval}",
                    $"{Constants.ButtonCommands.DATE_PICKER}:{Constants.START_INTERVAL_YEAR + (Constants.YEARS_INTERVAL * rowNum)}")
                };

                dateTimePicker.Add(row);
            }

            dateTimePicker.Add(new List<InlineKeyboardButton>
            {
                MainMenuHelper.GetCancelButton()
            });

            return new InlineKeyboardMarkup(dateTimePicker);
        }

        private InlineKeyboardMarkup GetYearKeyboard(DatePickerData datePickerData)
        {
            var dateTimePicker = new List<List<InlineKeyboardButton>>();
            var currentYear = DateTime.Now.Year;

            ///Max count rows <see cref="Constants.YEARS_INTERVAL"/> / <see cref="Constants.YEARS_PER_ROW"/> rows
            for (var rowNum = 0; rowNum < Constants.YEARS_INTERVAL / Constants.YEARS_PER_ROW; rowNum++)
            {
                var row = new List<InlineKeyboardButton>();
                int? yearNum = 0;

                ///Count columns <see cref="Constants.YEARS_PER_ROW"/>
                for (var columnNum = 0; columnNum < 5; columnNum++)
                {
                    yearNum = datePickerData?.MinYearInterval + rowNum * Constants.YEARS_PER_ROW + columnNum;

                    if (yearNum > currentYear)
                    {
                        row.Add(InlineKeyboardButton.WithCallbackData(
                        " ",
                        Constants.ButtonCommands.IGNORE));
                    }
                    else
                    {
                        row.Add(InlineKeyboardButton.WithCallbackData(
                        $"{yearNum}",
                        $"{Constants.ButtonCommands.DATE_PICKER}:{datePickerData?.MinYearInterval}:{yearNum}"));
                    }
                }

                dateTimePicker.Add(row);

                if (yearNum > currentYear)
                {
                    break;
                }
            }

            dateTimePicker.Add(new List<InlineKeyboardButton>
            {
                MainMenuHelper.GetCancelButton()
            });

            return new InlineKeyboardMarkup(dateTimePicker);
        }

        private InlineKeyboardMarkup GetMonthKeyboard(DatePickerData datePickerData)
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

                    row.Add(InlineKeyboardButton.WithCallbackData($"{dtFormatInfo.GetAbbreviatedMonthName(monthNum)}", $"{Constants.ButtonCommands.DATE_PICKER}:{datePickerData?.MinYearInterval}:{datePickerData?.DateTime?.Year}:{monthNum}"));
                }

                dateTimePicker.Add(row);
            }

            dateTimePicker.Add(new List<InlineKeyboardButton>
            {
                MainMenuHelper.GetCancelButton()
            });

            return new InlineKeyboardMarkup(dateTimePicker);
        }

        private InlineKeyboardMarkup GetDaysKeyboard(DatePickerData datePickerData)
        {
            if (datePickerData?.DateTime == null)
            {
                return null;
            }

            var dateTimePicker = new List<List<InlineKeyboardButton>>();

            var daysInMonth = DateTime.DaysInMonth(datePickerData.DateTime.Value.Year, datePickerData.DateTime.Value.Month);
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
                        row.Add(InlineKeyboardButton.WithCallbackData(" ", Constants.ButtonCommands.IGNORE));
                        continue;
                    }

                    row.Add(InlineKeyboardButton.WithCallbackData($"{currentDay}", $"{Constants.ButtonCommands.DATE_PICKER}:{datePickerData?.MinYearInterval}:{datePickerData?.DateTime?.Year}:{datePickerData?.DateTime?.Month}:{currentDay}"));

                    currentDay++;
                }

                dateTimePicker.Add(row);
            }

            dateTimePicker.Add(new List<InlineKeyboardButton>
            {
                MainMenuHelper.GetCancelButton()
            });

            return new InlineKeyboardMarkup(dateTimePicker);
        }

        private InlineKeyboardMarkup GetHourKeyboard(DatePickerData datePickerData)
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
                    var hourString = currentHour < 10 ? $"0{currentHour}" : currentHour.ToString();

                    row.Add(InlineKeyboardButton.WithCallbackData($"{hourString}", $"{Constants.ButtonCommands.DATE_PICKER}:{datePickerData?.MinYearInterval}:{datePickerData?.DateTime?.Year}:{datePickerData?.DateTime?.Month}:{datePickerData?.DateTime?.Day}:{currentHour}"));

                    currentHour++;
                }

                dateTimePicker.Add(row);
            }

            dateTimePicker.Add(new List<InlineKeyboardButton>
            {
                MainMenuHelper.GetCancelButton()
            });

            return new InlineKeyboardMarkup(dateTimePicker);
        }

        private InlineKeyboardMarkup GetMinuteKeyboard(DatePickerData datePickerData)
        {
            var dateTimePicker = new List<List<InlineKeyboardButton>>();

            var currentMinute = 0;

            //10 Rows
            for (var rowNum = 0; rowNum < 10; rowNum++)
            {
                var row = new List<InlineKeyboardButton>();

                //6 Cells per row
                for (var columnNum = 0; columnNum < 6; columnNum++)
                {
                    var minuteString = currentMinute < 10 ? $"0{currentMinute}" : currentMinute.ToString();

                    row.Add(InlineKeyboardButton.WithCallbackData($"{minuteString}", $"{Constants.ButtonCommands.DATE_PICKER}:{datePickerData?.MinYearInterval}:{datePickerData?.DateTime?.Year}:{datePickerData?.DateTime?.Month}:{datePickerData?.DateTime?.Day}:{datePickerData?.DateTime?.Hour}:{currentMinute}"));

                    currentMinute++;
                }

                dateTimePicker.Add(row);
            }

            dateTimePicker.Add(new List<InlineKeyboardButton>
            {
                MainMenuHelper.GetCancelButton()
            });

            return new InlineKeyboardMarkup(dateTimePicker);
        }

        private InlineKeyboardMarkup GetGmtKeyboard(DatePickerData datePickerData)
        {
            var dateTimePicker = new List<List<InlineKeyboardButton>>();

            //24 Rows
            for (var rowNum = 0; rowNum < 24; rowNum++)
            {
                var row = new List<InlineKeyboardButton>();

                var gmtSign = Constants.TIME_ZONE_DICT[rowNum].TimeZoneInt >= 0 ? "+" : "-";
                var buttonText = $"[GMT{gmtSign}{Math.Abs(Constants.TIME_ZONE_DICT[rowNum].TimeZoneInt)}] ({Constants.TIME_ZONE_DICT[rowNum].Description})";

                row.Add(InlineKeyboardButton.WithCallbackData(
                    buttonText,
                    $"{Constants.ButtonCommands.DATE_PICKER}:{datePickerData?.MinYearInterval}:{datePickerData?.DateTime?.Year}:{datePickerData?.DateTime?.Month}:{datePickerData?.DateTime?.Day}:{datePickerData?.DateTime?.Hour}:{datePickerData?.DateTime?.Minute}:{Constants.TIME_ZONE_DICT[rowNum].TimeZoneInt}"));

                dateTimePicker.Add(row);
            }

            dateTimePicker.Add(new List<InlineKeyboardButton>
            {
                MainMenuHelper.GetCancelButton()
            });

            return new InlineKeyboardMarkup(dateTimePicker);
        }
    }
}
