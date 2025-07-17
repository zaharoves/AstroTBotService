using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using AstroTBotService.Entities;

namespace AstroTBotService.TBot
{
    public class DatePicker : IDatePicker
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ITClientHelper _clientHelper;
        private readonly IResourcesLocaleManager _resourcesLocaleManager;
        private readonly IResourcesLocaleManager _localeManager;

        private readonly char _saparator = '_';

        public DatePicker(
            ITelegramBotClient botClient,
            ITClientHelper clientHelper,
            IResourcesLocaleManager resourcesLocaleManager,
            IResourcesLocaleManager localeManager)
        {
            _botClient = botClient;
            _clientHelper = clientHelper;
            _resourcesLocaleManager = resourcesLocaleManager;
            _localeManager = localeManager;
        }

        public bool TryParseDateTimePicker(TBotClientData clientData, out DatePickerData datePickerData)
        {
            datePickerData = new DatePickerData();

            try
            {
                if (clientData.CallbackData == null)
                {
                    return false;
                }

                var parts = clientData.CallbackData.Split(_saparator);

                var isSaveCommand = parts.Length > 8 && parts[8] == Constants.UI.ButtonCommands.SAVE_BIRTHDAY;
                var isChangeCommand = parts.Length > 8 && parts[8] == Constants.UI.ButtonCommands.CHANGE_BIRTHDAY;
                var isCancelCommand = parts.Length > 8 && (parts[8] == Constants.UI.ButtonCommands.SEND_MAIN_MENU || parts[8] == Constants.UI.ButtonCommands.EDIT_TO_MAIN_MENU);

                var gmtOffset = parts.Length > 7 && TimeSpan.TryParse(parts[7], out var _gmtOffset) ? _gmtOffset : TimeSpan.MinValue;

                var minute = parts.Length > 6 && int.TryParse(parts[6], out var _minute) ? _minute : 0;
                var hour = parts.Length > 5 && int.TryParse(parts[5], out var _hour) ? _hour : 1;
                var day = parts.Length > 4 && int.TryParse(parts[4], out var _day) ? _day : 1;
                var month = parts.Length > 3 && int.TryParse(parts[3], out var _month) ? _month : 1;
                var year = parts.Length > 2 && int.TryParse(parts[2], out var _year) ? _year : 1;
                var yearInterval = parts.Length > 1 && int.TryParse(parts[1], out var _yearInterval) ? _yearInterval : 1;

                var dateTime = new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Utc);

                datePickerData.MinYearInterval = yearInterval;
                datePickerData.DateTime = dateTime;
                datePickerData.GmtOffset = gmtOffset;

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

        public async Task SendYearIntervalPicker(TBotClientData clientData, string text)
        {
            var keyboard = GetYearIntervalKeyboard(clientData);

            await _botClient.SendMessage(
                chatId: clientData.ChatId,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendYearPicker(TBotClientData clientData, string text)
        {
            var keyboard = GetYearKeyboard(clientData);

            await _botClient.EditMessageText(
                chatId: clientData.ChatId,
                messageId: clientData.Message.Id,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendMonthPicker(TBotClientData clientData, string text)
        {
            var keyboard = GetMonthKeyboard(clientData);

            await _botClient.EditMessageText(
                chatId: clientData.ChatId,
                messageId: clientData.Message.Id,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendDayPicker(TBotClientData clientData, string text)
        {
            var keyboard = GetDaysKeyboard(clientData);

            await _botClient.EditMessageText( //EditMessageReplyMarkup??
                chatId: clientData.ChatId,
                messageId: clientData.Message.Id,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendHourPicker(TBotClientData clientData, string text)
        {
            var keyboard = GetHourKeyboard(clientData);

            await _botClient.EditMessageText(
                chatId: clientData.ChatId,
                messageId: clientData.Message.Id,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendMinutePicker(TBotClientData clientData, string text)
        {
            var keyboard = GetMinuteKeyboard(clientData);

            await _botClient.EditMessageText(
                chatId: clientData.ChatId,
                messageId: clientData.Message.Id,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendTimeZonePicker(TBotClientData clientData, string text)
        {
            var keyboard = GetGmtKeyboard(clientData);

            await _botClient.EditMessageText(
                chatId: clientData.ChatId,
                messageId: clientData.Message.Id,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendConfirmDate(TBotClientData clientData, string text)
        {
            var prefixCommand = $"{Constants.UI.ButtonCommands.DATE_PICKER}" +
                $"{_saparator}{clientData?.DatePickerData?.MinYearInterval}" +
                $"{_saparator}{clientData?.DatePickerData?.DateTime?.Year}" +
                $"{_saparator}{clientData?.DatePickerData?.DateTime?.Month}" +
                $"{_saparator}{clientData?.DatePickerData?.DateTime?.Day}" +
                $"{_saparator}{clientData?.DatePickerData?.DateTime?.Hour}" +
                $"{_saparator}{clientData?.DatePickerData?.DateTime?.Minute}" +
                $"{_saparator}{clientData?.DatePickerData?.GmtOffset}";

            // Создаем список рядов кнопок
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"{_resourcesLocaleManager.GetString("Save", clientData.CultureInfo)} {Constants.UI.Icons.Common.GREEN_CIRCLE}", 
                        $"{prefixCommand}" +
                        $"{_saparator}{Constants.UI.ButtonCommands.SAVE_BIRTHDAY}"),

                    InlineKeyboardButton.WithCallbackData(
                        $"{_resourcesLocaleManager.GetString("Change", clientData.CultureInfo)} {Constants.UI.Icons.Common.YELLOW_CIRCLE}", 
                        $"{prefixCommand}" +
                        $"{_saparator}{Constants.UI.ButtonCommands.CHANGE_BIRTHDAY}"),
                },
                new []
                {
                    _clientHelper.GetCancelButtonWithEdit(clientData)
                }
            });

            await _botClient.EditMessageText(
                chatId: clientData.ChatId,
                messageId: clientData.Message.Id,
                text: text,
                replyMarkup: inlineKeyboard);
        }

        private InlineKeyboardMarkup GetYearIntervalKeyboard(TBotClientData clientData)
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
                    $"{Constants.UI.ButtonCommands.DATE_PICKER}" +
                    $"{_saparator}{Constants.START_INTERVAL_YEAR + (Constants.YEARS_INTERVAL * rowNum)}")
                };

                dateTimePicker.Add(row);
            }

            dateTimePicker.Add(new List<InlineKeyboardButton>
            {
                _clientHelper.GetCancelButtonWithEdit(clientData)
            });

            return new InlineKeyboardMarkup(dateTimePicker);
        }

        private InlineKeyboardMarkup GetYearKeyboard(TBotClientData clientData)
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
                    yearNum = clientData?.DatePickerData?.MinYearInterval + rowNum * Constants.YEARS_PER_ROW + columnNum;

                    if (yearNum > currentYear)
                    {
                        row.Add(InlineKeyboardButton.WithCallbackData(
                        " ",
                        Constants.UI.ButtonCommands.IGNORE));
                    }
                    else
                    {
                        row.Add(InlineKeyboardButton.WithCallbackData(
                        $"{yearNum}",
                        $"{Constants.UI.ButtonCommands.DATE_PICKER}" +
                        $"{_saparator}{clientData?.DatePickerData?.MinYearInterval}" +
                        $"{_saparator}{yearNum}"));
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
                _clientHelper.GetCancelButtonWithEdit(clientData)
            });

            return new InlineKeyboardMarkup(dateTimePicker);
        }

        private InlineKeyboardMarkup GetMonthKeyboard(TBotClientData clientData)
        {
            var dateTimePicker = new List<List<InlineKeyboardButton>>();

            DateTimeFormatInfo dtFormatInfo = clientData.CultureInfo.DateTimeFormat;

            //4 Rows
            for (var rowNum = 0; rowNum < 4; rowNum++)
            {
                var row = new List<InlineKeyboardButton>();

                //3 Months per row
                for (var columnNum = 0; columnNum < 3; columnNum++)
                {
                    var monthNum = rowNum * 3 + columnNum + 1;

                    row.Add(InlineKeyboardButton.WithCallbackData(
                        $"{dtFormatInfo.GetAbbreviatedMonthName(monthNum)}", 
                        $"{Constants.UI.ButtonCommands.DATE_PICKER}" +
                        $"{_saparator}{clientData?.DatePickerData?.MinYearInterval}" +
                        $"{_saparator}{clientData?.DatePickerData?.DateTime?.Year}" +
                        $"{_saparator}{monthNum}"));
                }

                dateTimePicker.Add(row);
            }

            dateTimePicker.Add(new List<InlineKeyboardButton>
            {
                _clientHelper.GetCancelButtonWithEdit(clientData)
            });

            return new InlineKeyboardMarkup(dateTimePicker);
        }

        private InlineKeyboardMarkup GetDaysKeyboard(TBotClientData clientData)
        {
            if (clientData.DatePickerData?.DateTime == null)
            {
                return null;
            }

            var dateTimePicker = new List<List<InlineKeyboardButton>>();

            var daysInMonth = DateTime.DaysInMonth(clientData.DatePickerData.DateTime.Value.Year, clientData.DatePickerData.DateTime.Value.Month);
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
                        row.Add(InlineKeyboardButton.WithCallbackData(" ", Constants.UI.ButtonCommands.IGNORE));
                        continue;
                    }

                    row.Add(InlineKeyboardButton.WithCallbackData(
                        $"{currentDay}", 
                        $"{Constants.UI.ButtonCommands.DATE_PICKER}" +
                        $"{_saparator}{clientData?.DatePickerData?.MinYearInterval}" +
                        $"{_saparator}{clientData?.DatePickerData?.DateTime?.Year}" +
                        $"{_saparator}{clientData?.DatePickerData?.DateTime?.Month}" +
                        $"{_saparator}{currentDay}"));

                    currentDay++;
                }

                dateTimePicker.Add(row);
            }

            dateTimePicker.Add(new List<InlineKeyboardButton>
            {
                _clientHelper.GetCancelButtonWithEdit(clientData)
            });

            return new InlineKeyboardMarkup(dateTimePicker);
        }

        private InlineKeyboardMarkup GetHourKeyboard(TBotClientData clientData)
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

                    row.Add(InlineKeyboardButton.WithCallbackData(
                        $"{hourString}", 
                        $"{Constants.UI.ButtonCommands.DATE_PICKER}" +
                        $"{_saparator}{clientData?.DatePickerData?.MinYearInterval}" +
                        $"{_saparator}{clientData?.DatePickerData?.DateTime?.Year}" +
                        $"{_saparator}{clientData?.DatePickerData?.DateTime?.Month}" +
                        $"{_saparator}{clientData?.DatePickerData?.DateTime?.Day}" +
                        $"{_saparator}{currentHour}"));

                    currentHour++;
                }

                dateTimePicker.Add(row);
            }

            dateTimePicker.Add(new List<InlineKeyboardButton>
            {
                _clientHelper.GetCancelButtonWithEdit(clientData)
            });

            return new InlineKeyboardMarkup(dateTimePicker);
        }

        private InlineKeyboardMarkup GetMinuteKeyboard(TBotClientData clientData)
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

                    row.Add(InlineKeyboardButton.WithCallbackData(
                        $"{minuteString}", 
                        $"{Constants.UI.ButtonCommands.DATE_PICKER}" +
                        $"{_saparator}{clientData?.DatePickerData?.MinYearInterval}" +
                        $"{_saparator}{clientData?.DatePickerData?.DateTime?.Year}" +
                        $"{_saparator}{clientData?.DatePickerData?.DateTime?.Month}" +
                        $"{_saparator}{clientData?.DatePickerData?.DateTime?.Day}" +
                        $"{_saparator}{clientData?.DatePickerData?.DateTime?.Hour}" +
                        $"{_saparator}{currentMinute}"));

                    currentMinute++;
                }

                dateTimePicker.Add(row);
            }

            dateTimePicker.Add(new List<InlineKeyboardButton>
            {
                _clientHelper.GetCancelButtonWithEdit(clientData)
            });

            return new InlineKeyboardMarkup(dateTimePicker);
        }

        private InlineKeyboardMarkup GetGmtKeyboard(TBotClientData clientData)
        {
            var dateTimePicker = new List<List<InlineKeyboardButton>>();

            foreach(var timeZone in Constants.TIME_ZONE_DICT)
            {
                var row = new List<InlineKeyboardButton>();

                var buttonText = GetTimeZoneDescription(timeZone, clientData.CultureInfo);

                row.Add(InlineKeyboardButton.WithCallbackData(
                    buttonText,
                    $"{Constants.UI.ButtonCommands.DATE_PICKER}" +
                    $"{_saparator}{clientData?.DatePickerData?.MinYearInterval}" +
                    $"{_saparator}{clientData?.DatePickerData?.DateTime?.Year}" +
                    $"{_saparator}{clientData?.DatePickerData?.DateTime?.Month}" +
                    $"{_saparator}{clientData?.DatePickerData?.DateTime?.Day}" +
                    $"{_saparator}{clientData?.DatePickerData?.DateTime?.Hour}" +
                    $"{_saparator}{clientData?.DatePickerData?.DateTime?.Minute}" +
                    $"{_saparator}{timeZone}"));
                //TODO
               

                dateTimePicker.Add(row);
            }

            dateTimePicker.Add(new List<InlineKeyboardButton>
            {
                _clientHelper.GetCancelButtonWithEdit(clientData)
            });

            return new InlineKeyboardMarkup(dateTimePicker);
        }

        private string GetTimeZoneDescription(TimeSpan utcTimeSpan, CultureInfo cultureInfo)
        {
            var gmtSign = utcTimeSpan.Hours >= 0 ? "+" : "-";

            string key = $"UTC{gmtSign}{Math.Abs(utcTimeSpan.Hours)}";

            if (utcTimeSpan.Minutes != 0)
            {
                key += $":{Math.Abs(utcTimeSpan.Minutes)}";
            }

            var descr = _localeManager.GetString(key, cultureInfo);

            return $"[{key}] {descr}";
        }
    }
}
