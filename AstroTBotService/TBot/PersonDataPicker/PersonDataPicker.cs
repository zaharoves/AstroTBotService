using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using AstroTBotService.Entities;

namespace AstroTBotService.TBot
{
    public class PersonDataPicker : IPersonDataPicker
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ITClientHelper _clientHelper;
        private readonly IResourcesLocaleManager _resourcesLocaleManager;
        private readonly IResourcesLocaleManager _localeManager;

        public PersonDataPicker(
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

        public async Task SendNamePicker(TBotClientData clientData, string text)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    _clientHelper.GetBackToPersonsButton(clientData),
                    _clientHelper.GetCancelButtonWithEdit(clientData)
                }
            });

            await _clientHelper.SendMessage(
                clientData.AstroUserId,
                $"{text}\n\n" +
                $"{Constants.UI.Icons.Common.MOON_1} {_localeManager.GetString("EnterPersonName", clientData.AstroUser.CultureInfo)}:",
                keyboard);
        }

        public async Task EditToNamePicker(TBotClientData clientData, string text)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    _clientHelper.GetCancelButtonWithEdit(clientData)
                }
            });

            await _clientHelper.EditMessage(
                clientData.AstroUserId,
                clientData.Message.Id,
                $"{text}\n\n" +
                $"{Constants.UI.Icons.Common.MOON_1} {_localeManager.GetString("EnterPersonName", clientData.AstroUser.CultureInfo)}:",
                keyboard);
        }

        public async Task SendLocationPicker(TBotClientData clientData, string text)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    _clientHelper.GetCancelButtonWithEdit(clientData)
                }
            });

            await _botClient.SendMessage(
                chatId: clientData.AstroUserId,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task EditToLocationPicker(TBotClientData clientData, string text)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    _clientHelper.GetCancelButtonWithEdit(clientData)
                }
            });

            await _botClient.EditMessageText(
                chatId: clientData.AstroUserId,
                messageId: clientData.Message.Id,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task EditToYearIntervalPicker(TBotClientData clientData, string text)
        {
            var keyboard = GetYearIntervalKeyboard(clientData);

            await _botClient.EditMessageText(
                chatId: clientData.AstroUserId,
                messageId: clientData.Message.Id,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendYearIntervalPicker(TBotClientData clientData, string text)
        {
            var keyboard = GetYearIntervalKeyboard(clientData);

            await _botClient.SendMessage(
                chatId: clientData.AstroUserId,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendYearPicker(TBotClientData clientData, string text)
        {
            var keyboard = GetYearKeyboard(clientData);

            await _botClient.EditMessageText(
                chatId: clientData.AstroUserId,
                messageId: clientData.Message.Id,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendMonthPicker(TBotClientData clientData, string text)
        {
            var keyboard = GetMonthKeyboard(clientData);

            await _botClient.EditMessageText(
                chatId: clientData.AstroUserId,
                messageId: clientData.Message.Id,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendDayPicker(TBotClientData clientData, string text)
        {
            var keyboard = GetDaysKeyboard(clientData);

            await _botClient.EditMessageText(
                chatId: clientData.AstroUserId,
                messageId: clientData.Message.Id,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendHourPicker(TBotClientData clientData, string text)
        {
            var keyboard = GetHourKeyboard(clientData);

            await _botClient.EditMessageText(
                chatId: clientData.AstroUserId,
                messageId: clientData.Message.Id,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendMinutePicker(TBotClientData clientData, string text)
        {
            var keyboard = GetMinuteKeyboard(clientData);

            await _botClient.EditMessageText(
                chatId: clientData.AstroUserId,
                messageId: clientData.Message.Id,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendTimeZonePicker(TBotClientData clientData, string text)
        {
            var keyboard = GetGmtKeyboard(clientData);

            await _botClient.EditMessageText(
                chatId: clientData.AstroUserId,
                messageId: clientData.Message.Id,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task SendConfirmDate(TBotClientData clientData, string text)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"{_resourcesLocaleManager.GetString("Save", clientData.AstroUser.CultureInfo)} {Constants.UI.Icons.Common.GREEN_CIRCLE}",
                        $"{Constants.UI.Buttons.Commands.SAVE_BIRTHDAY}"),

                    InlineKeyboardButton.WithCallbackData(
                        $"{_resourcesLocaleManager.GetString("Change", clientData.AstroUser.CultureInfo)} {Constants.UI.Icons.Common.YELLOW_CIRCLE}",
                        $"{Constants.UI.Buttons.Commands.CHANGE_BIRTHDAY}"),
                },
                new []
                {
                    _clientHelper.GetCancelButtonWithEdit(clientData)
                }
            });

            await _botClient.SendMessage(
                chatId: clientData.AstroUserId,
                text: text,
                replyMarkup: keyboard);
        }

        public async Task EditToConfirmDate(TBotClientData clientData, string text)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"{_resourcesLocaleManager.GetString("Save", clientData.AstroUser.CultureInfo)} {Constants.UI.Icons.Common.GREEN_CIRCLE}",
                        $"{Constants.UI.Buttons.Commands.SAVE_BIRTHDAY}"),

                    InlineKeyboardButton.WithCallbackData(
                        $"{_resourcesLocaleManager.GetString("Change", clientData.AstroUser.CultureInfo)} {Constants.UI.Icons.Common.YELLOW_CIRCLE}",
                        $"{Constants.UI.Buttons.Commands.CHANGE_BIRTHDAY}"),
                },
                new []
                {
                    _clientHelper.GetCancelButtonWithEdit(clientData)
                }
            });

            await _botClient.EditMessageText(
                chatId: clientData.AstroUserId,
                messageId: clientData.Message.Id,
                text: text,
                replyMarkup: keyboard);
        }

        private InlineKeyboardMarkup GetYearIntervalKeyboard(TBotClientData clientData)
        {
            var dateTimePicker = new List<List<InlineKeyboardButton>>();

            var currentYear = DateTime.Now.Year;
            var intervalsCount = ((currentYear - Constants.UI.START_INTERVAL_YEAR) / Constants.UI.YEARS_INTERVAL) + 1;

            for (var rowNum = 0; rowNum < intervalsCount; rowNum++)
            {
                var startInterval = $"{Constants.UI.START_INTERVAL_YEAR + (Constants.UI.YEARS_INTERVAL * rowNum)}";
                var endInterval = rowNum == intervalsCount - 1
                    ? currentYear.ToString()
                    : $"{Constants.UI.START_INTERVAL_YEAR + (Constants.UI.YEARS_INTERVAL * (rowNum + 1)) - 1}";

                var row = new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData(
                    $"{startInterval} - {endInterval}",
                    $"{Constants.UI.START_INTERVAL_YEAR + (Constants.UI.YEARS_INTERVAL * rowNum)}")
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

            ///Max count rows <see cref="Constants.UI.YEARS_INTERVAL"/> / <see cref="Constants.UI.YEARS_PER_ROW"/> rows
            for (var rowNum = 0; rowNum < Constants.UI.YEARS_INTERVAL / Constants.UI.YEARS_PER_ROW; rowNum++)
            {
                var row = new List<InlineKeyboardButton>();
                int? yearNum = 0;

                ///Count columns <see cref="Constants.UI.YEARS_PER_ROW"/>
                for (var columnNum = 0; columnNum < Constants.UI.YEARS_PER_ROW; columnNum++)
                {
                    yearNum = clientData?.RedisPersonData?.StartYearInterval + rowNum * Constants.UI.YEARS_PER_ROW + columnNum;

                    if (yearNum > currentYear)
                    {
                        row.Add(InlineKeyboardButton.WithCallbackData(
                        " ",
                        Constants.UI.Buttons.Commands.IGNORE));
                    }
                    else
                    {
                        row.Add(InlineKeyboardButton.WithCallbackData(
                        $"{yearNum}",
                        $"{yearNum}"));
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

            DateTimeFormatInfo dtFormatInfo = clientData.AstroUser.CultureInfo.DateTimeFormat;

            //4 Rows
            for (var rowNum = 0; rowNum < Constants.UI.MONTHS_PER_ROW; rowNum++)
            {
                var row = new List<InlineKeyboardButton>();

                //3 Months per row
                for (var columnNum = 0; columnNum < (Constants.UI.MONTHS_COUNT / Constants.UI.MONTHS_PER_ROW); columnNum++)
                {
                    var monthNum = rowNum * 3 + columnNum + 1;

                    row.Add(InlineKeyboardButton.WithCallbackData(
                        $"{dtFormatInfo.GetAbbreviatedMonthName(monthNum)}",
                        $"{monthNum}"));
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
            if (clientData.RedisPersonData?.GetDateTimeOffset() == null)
            {
                return null;
            }

            var dateTimePicker = new List<List<InlineKeyboardButton>>();

            var daysInMonth = DateTime.DaysInMonth(clientData.RedisPersonData.GetDateTimeOffset().Year, clientData.RedisPersonData.GetDateTimeOffset().Month);
            var currentDay = 1;

            for (var rowNum = 0; rowNum < Constants.UI.DAYS_COLUMNS_COUNT; rowNum++)
            {
                var row = new List<InlineKeyboardButton>();

                if (currentDay > daysInMonth)
                {
                    break;
                }

                for (var columnNum = 0; columnNum < Constants.UI.DAYS_PER_ROW; columnNum++)
                {
                    if (currentDay > daysInMonth)
                    {
                        row.Add(InlineKeyboardButton.WithCallbackData(" ", Constants.UI.Buttons.Commands.IGNORE));
                        continue;
                    }

                    row.Add(InlineKeyboardButton.WithCallbackData(
                        $"{currentDay}",
                        $"{currentDay}"));

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

            for (var rowNum = 0; rowNum < (Constants.UI.HOURS_COUNT / Constants.UI.HOURS_PER_ROW); rowNum++)
            {
                var row = new List<InlineKeyboardButton>();

                for (var columnNum = 0; columnNum < Constants.UI.HOURS_PER_ROW; columnNum++)
                {
                    var hourString = currentHour < 10 ? $"0{currentHour}" : currentHour.ToString();

                    row.Add(InlineKeyboardButton.WithCallbackData(
                        $"{hourString}",
                        $"{currentHour}"));

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

            for (var rowNum = 0; rowNum < (Constants.UI.MINUTES_COUNT / Constants.UI.MINUTES_PER_ROW); rowNum++)
            {
                var row = new List<InlineKeyboardButton>();

                for (var columnNum = 0; columnNum < Constants.UI.MINUTES_PER_ROW; columnNum++)
                {
                    var minuteString = currentMinute < 10 ? $"0{currentMinute}" : currentMinute.ToString();

                    row.Add(InlineKeyboardButton.WithCallbackData(
                        $"{minuteString}",
                        $"{currentMinute}"));

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

            foreach (var timeZone in Constants.TIME_ZONE_DICT)
            {
                var row = new List<InlineKeyboardButton>();

                var buttonText = GetTimeZoneDescription(timeZone, clientData.AstroUser.CultureInfo);

                row.Add(InlineKeyboardButton.WithCallbackData(
                    buttonText,
                    $"{timeZone}"));

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
