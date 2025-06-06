using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Globalization;
using Telegram.Bot.Polling;
using AstroTBotService.RMQ;
using AstroTBotService.Enums;
using AstroHandlerService.Db.Providers;
using AstroTBotService.Entities;
using AstroHandlerService.Db.Entities;

namespace AstroTBotService.TBot
{
    public class TBotUpdateHandler : IUpdateHandler
    {
        //rmqMessageId, chatId 
        public static Dictionary<string, long> RmqDict = new Dictionary<string, long>();

        private readonly ITelegramBotClient _botClient;
        private readonly IMainMenuHelper _mainMenuHelper;
        private readonly IDatePicker _datePicker;
        private readonly IRmqProducer _rmqProducer;
        private readonly IUserProvider _userProvider;
        private readonly IResourcesLocaleManager _localeManager;

        public TBotClientData ClientData { get; private set; }

        public TBotUpdateHandler(
            ITelegramBotClient botClient,
            IMainMenuHelper mainMenuHelper,
            IDatePicker tBotDatePicker,
            IRmqProducer rmqProducer,
            IUserProvider userProvider,
            IResourcesLocaleManager localeManager)
        {
            _botClient = botClient;
            _mainMenuHelper = mainMenuHelper;
            _datePicker = tBotDatePicker;
            _rmqProducer = rmqProducer;
            _userProvider = userProvider;
            _localeManager = localeManager;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var astroUserInfo = await GetOrCreateUser(update);
            var message = update?.Message ?? update?.CallbackQuery?.Message;
            var callbackData = update?.CallbackQuery?.Data;

            ClientData = new TBotClientData()
            {
                AstroUser = astroUserInfo.AstroUser,
                CultureInfo = astroUserInfo.CultureInfo,
                Message = message,
                CallbackData = callbackData
            };

            if (update?.Message != null)
            {
                await HandleMessageAsync();
            }
            else if (update?.CallbackQuery != null)
            {
                await HandleCallbackAsync();
            }
        }

        private async Task HandleMessageAsync()
        {
            if (ClientData?.Message?.Type != MessageType.Text)
            {
                //TODO
                return;
            }

            if (ClientData?.Message?.Text?.ToLower() == Constants.MessageCommands.START)
            {
                await _mainMenuHelper.SendMainMenu(ClientData);
                await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.MainMenu.ToString());

                return;
            }
        }

        private async Task HandleCallbackAsync()
        {
            if (ClientData.CallbackData == null ||
                ClientData.CallbackData == Constants.ButtonCommands.IGNORE)
            {
                return;
            }

            switch (ClientData.CallbackData)
            {
                case Constants.ButtonCommands.TO_MAIN_MENU:
                    await _mainMenuHelper.EditToMainMenu(ClientData);
                    await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.MainMenu.ToString());
                    return;
                case Constants.ButtonCommands.TODAY_FORECAST:
                    var userInfo = await _userProvider.GetUser(ClientData.ChatId);

                    if (userInfo == null)
                    {
                        return;
                    }

                    await _botClient.SendMessage(
                            chatId: ClientData.ChatId,
                            text: $"{Constants.Icons.Common.HOURGLASS} {_localeManager.GetString("ForecastProccessing", ClientData.CultureInfo)}",
                            replyMarkup: null);

                    var messageGuid = Guid.NewGuid().ToString();

                    var rmqMessage = new UserInfoMessage()
                    {
                        MessageId = messageGuid,
                        DateTime = userInfo.BirthDate,
                        GmtOffset = userInfo.GmtOffset
                    };

                    _rmqProducer.SendMessage(messageGuid, rmqMessage);
                    RmqDict.Add(messageGuid, ClientData.ChatId);

                    return;
                case Constants.ButtonCommands.CHANGE_LANGUAGE:
                    await _mainMenuHelper.SendLanguagePicker(ClientData);
                    await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.LanguagePicker.ToString());
                    return;
                case Constants.ButtonCommands.POSITIVE_FORECAST:

                    return;

            }


            #region DatePicker

            if (_datePicker.TryParseDateTimePicker(ClientData, out var datePickerData))
            {
                ClientData.DatePickerData = datePickerData;
            }
            else
            {
                //TODO
            }

            var userStage = _userProvider.GetUserStage(ClientData.ChatId).Result;
            var userStageEnum = (ChatStageEnum)Enum.Parse(typeof(ChatStageEnum), userStage?.Stage, true);

            switch (userStageEnum)
            {
                case ChatStageEnum.MainMenu:
                    switch (ClientData.CallbackData)
                    {
                        case Constants.ButtonCommands.SET_BIRTHDATE:
                            await _datePicker.SendYearIntervalPicker(
                                ClientData,
                                $"{_localeManager.GetString("ChooseBirthYear", ClientData.CultureInfo)}:");

                            await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.YearIntervalPicker.ToString());
                            break;
                        case Constants.ButtonCommands.TODAY_FORECAST:
                            break;
                        case Constants.ButtonCommands.POSITIVE_FORECAST:
                            break;
                    }
                    return;

                case ChatStageEnum.LanguagePicker:
                    if (Constants.FlagsInfoDict.TryGetValue(ClientData.CallbackData, out (string Inon, string Descr) languageInfo))
                    {
                        ClientData.AstroUser.Language = ClientData.CallbackData;
                        ClientData.CultureInfo = new CultureInfo(ClientData.CallbackData);

                        await _userProvider.EditUser(ClientData.AstroUser.Id.Value, ClientData.AstroUser);
                    }

                    await _mainMenuHelper.EditToMainMenu(ClientData);
                    await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.MainMenu.ToString());
                    return;

                case ChatStageEnum.YearIntervalPicker:
                    await _datePicker.SendYearPicker(
                        ClientData,
                        $"{_localeManager.GetString("ChooseBirthYear", ClientData.CultureInfo)}:");

                    await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.YearPicker.ToString());
                    return;

                case ChatStageEnum.YearPicker:
                    await _datePicker.SendMonthPicker(
                        ClientData,
                        $"{_localeManager.GetString("ChooseBirthMonth", ClientData.CultureInfo)}:");

                    await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.MonthPicker.ToString());
                    return;

                case ChatStageEnum.MonthPicker:
                    await _datePicker.SendDayPicker(
                        ClientData,
                        $"{_localeManager.GetString("ChooseBirthDay", ClientData.CultureInfo)}:\n" +
                        $"[ {ClientData.DatePickerData?.DateTime.Value.ToString("MMMM yyyy", ClientData.CultureInfo)} ]");

                    await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.DayPicker.ToString());
                    return;

                case ChatStageEnum.DayPicker:
                    await _datePicker.SendHourPicker(
                        ClientData,
                        $"{_localeManager.GetString("ChooseBirthHour", ClientData.CultureInfo)}:\n" +
                        $"[ {ClientData.DatePickerData?.DateTime.Value.ToString("d MMMM yyyy", ClientData.CultureInfo)} ]");

                    await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.HourPicker.ToString());
                    return;

                case ChatStageEnum.HourPicker:
                    await _datePicker.SendMinutePicker(
                        ClientData,
                        $"{_localeManager.GetString("ChooseBirthMinute", ClientData.CultureInfo)}:\n" +
                        $"[ {ClientData.DatePickerData?.DateTime.Value.ToString("d MMMM yyyy", ClientData.CultureInfo)} " +
                        $"{Constants.Icons.Common.MINUS} " +
                        $"{ClientData.DatePickerData?.DateTime.Value.ToString("HH:XX", ClientData.CultureInfo)} ]");

                    await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.MinutePicker.ToString());
                    return;

                case ChatStageEnum.MinutePicker:
                    await _datePicker.SendTimeZonePicker(
                        ClientData,
                        $"{_localeManager.GetString("ChooseTimeZone", ClientData.CultureInfo)}: \n" +
                        $"[ {ClientData.DatePickerData?.DateTime.Value.ToString("d MMMM yyyy", ClientData.CultureInfo)} " +
                        $"{Constants.Icons.Common.MINUS} " +
                        $"{ClientData.DatePickerData?.DateTime.Value.ToString("HH:mm", ClientData.CultureInfo)} ]");

                    await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.TimeZonePicker.ToString());
                    return;

                case ChatStageEnum.TimeZonePicker:
                    var gmtSign = ClientData.DatePickerData?.GmtOffset >= TimeSpan.Zero ? "+" : "-";
                    var gmtStr = ClientData.DatePickerData?.GmtOffset.Minutes == 0 ?
                        $"[GMT{gmtSign}{Math.Abs(ClientData.DatePickerData.GmtOffset.Hours)}]" :
                        $"[GMT{gmtSign}{Math.Abs(ClientData.DatePickerData.GmtOffset.Hours)}:{Math.Abs(ClientData.DatePickerData.GmtOffset.Minutes)}]";

                    await _datePicker.SendConfirmDate(
                        ClientData,
                        $"{Constants.Icons.Common.SCIENCE} " +
                        $"{_localeManager.GetString("YourBirthDate", ClientData.CultureInfo)}:\n" +
                        $"{ClientData.DatePickerData?.DateTime.Value.ToString("d MMMM yyyy", ClientData.CultureInfo)} " +
                        $"{Constants.Icons.Common.MINUS} " +
                        $"{ClientData.DatePickerData?.DateTime.Value.ToString("HH:mm", ClientData.CultureInfo)} " +
                        $"{gmtStr}");

                    await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.ConfirmBirthday.ToString());
                    return;

                case ChatStageEnum.ConfirmBirthday:
                    if (ClientData.DatePickerData?.IsSaveCommand ?? false)
                    {
                        var editUserInfo = new AstroUser()
                        {
                            BirthDate = ClientData.DatePickerData.DateTime,
                            GmtOffset = ClientData.DatePickerData.GmtOffset,
                            Language = ClientData.CultureInfo.Name
                        };

                        await _userProvider.EditUser(ClientData.ChatId, editUserInfo);

                        await _mainMenuHelper.EditToMainMenu(ClientData);

                        await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.MainMenu.ToString());
                    }
                    else if (ClientData.DatePickerData?.IsChangeCommand ?? false)
                    {
                        await _datePicker.SendYearPicker(
                            ClientData,
                            $"{_localeManager.GetString("ChooseBirthYear", ClientData.CultureInfo)}:");

                        await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.YearPicker.ToString());
                    }
                    else if (ClientData.DatePickerData?.IsCancelCommand ?? false)
                    {
                        await _mainMenuHelper.SendMainMenu(ClientData);
                        await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.MainMenu.ToString());
                    }

                    return;
            }

            #endregion
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task<(AstroUser? AstroUser, CultureInfo CultureInfo)> GetOrCreateUser(Update update)
        {
            var chatId = update?.CallbackQuery?.Message?.Chat?.Id
                ?? update?.Message?.Chat?.Id;

            if (!chatId.HasValue)
            {
                //TODO
                var a = new AstroUser();
                var b = CultureInfo.CurrentCulture;

                return new(a, b);
            }

            var astroUser = await _userProvider.GetUser(chatId.Value);
            var cultureInfo = GetUserCultureInfo(astroUser, update);

            if (astroUser == null)
            {
                astroUser = new AstroUser()
                {
                    Id = chatId,
                    BirthDate = null,
                    GmtOffset = null,
                    Language = cultureInfo.Name
                };

                await _userProvider.AddUser(astroUser);
            }

            return (astroUser, cultureInfo);
        }

        public CultureInfo GetUserCultureInfo(AstroUser astroUser, Update update)
        {
            if (!string.IsNullOrEmpty(astroUser?.Language))
            {
                return new CultureInfo(astroUser.Language);
            }

            var language = update?.CallbackQuery?.From?.LanguageCode
                ?? update?.Message?.From?.LanguageCode
                ?? "en";

            if (Constants.LocaleDict.TryGetValue(language, out var cultureInfo))
            {
                return cultureInfo;
            }

            return new CultureInfo("en-US");
        }
    }
}
