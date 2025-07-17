using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Globalization;
using Telegram.Bot.Polling;
using AstroTBotService.Enums;
using AstroTBotService.Entities;
using AstroTBotService.Db.Providers;
using AstroTBotService.Db.Entities;
using AstroTBotService.AstroCalculation.Services;
using Telegram.Bot.Types.ReplyMarkups;
using Serilog.Context;
using Serilog.Debugging;
using Serilog;
using System;

namespace AstroTBotService.TBot
{
    public class TBotUpdateHandler : IUpdateHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ITClientHelper _clientHelper;
        private readonly IDatePicker _datePicker;
        private readonly ILocationPicker _locationPicker;
        private readonly IUserProvider _userProvider;
        private readonly IResourcesLocaleManager _localeManager;
        private readonly ICalculationService _calculationService;
        private readonly ILogger<TBotUpdateHandler> _logger;

        public TBotClientData ClientData { get; private set; }

        public TBotUpdateHandler(
            ITelegramBotClient botClient,
            ITClientHelper clientHelper,
            IDatePicker tBotDatePicker,
            ILocationPicker locationPicker,
            IUserProvider userProvider,
            ICalculationService calculationService,
            IResourcesLocaleManager localeManager,
            ILogger<TBotUpdateHandler> logger)
        {
            _botClient = botClient;
            _clientHelper = clientHelper;
            _datePicker = tBotDatePicker;
            _locationPicker = locationPicker;
            _userProvider = userProvider;
            _calculationService = calculationService;
            _localeManager = localeManager;
            _logger = logger;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                using (LogContext.PushProperty("OrderId", 1234)) // local - only logs within this using block will have OrderId
                {
                    _logger.LogInformation("Hello, versioned Serilog!{OrderId}{asdasd}", "111", "222");
                    _logger.LogInformation("Hello, ordered Serilog!");
                }

                _logger.LogTrace("1");
                _logger.LogDebug("2");
                _logger.LogInformation("3");
                _logger.LogWarning("4");
                _logger.LogError("5");
                _logger.LogCritical("6");

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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _logger.LogError($"{ex.Message}\n{ex.StackTrace}");
            }
        }

        private async Task HandleMessageAsync()
        {
            if (ClientData?.Message?.Type != MessageType.Text && ClientData?.Message?.Type != MessageType.Location)
            {
                //TODO
                return;
            }

            if (ClientData?.Message?.Text?.ToLower() == Constants.UI.MessageCommands.START)
            {
                await _clientHelper.SendMainMenu(ClientData);
                await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.MainMenu);

                return;
            }
            else if (ClientData?.Message?.Text?.ToLower() == Constants.UI.MessageCommands.SET_BIRTHDATE)
            {
                await SetBirthday();
                return;
            }
            else if (ClientData?.Message?.Text?.ToLower() == Constants.UI.MessageCommands.SET_LOCATION)
            {
                await SetLocation();
                return;
            }
            else if (ClientData?.Message?.Text?.ToLower() == Constants.UI.MessageCommands.SET_LANGUAGE)
            {
                await SetLanguage();
                return;
            }
            else if (ClientData?.Message?.Text?.ToLower() == Constants.UI.MessageCommands.SET_HOUSES_SYSTEM)
            {
                await SetHouseSystem();
                return;
            }


            switch (ClientData.CallbackData)
            {
                case Constants.UI.ButtonCommands.SEND_MAIN_MENU:
                    await _clientHelper.SendMainMenu(ClientData);
                    await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.MainMenu);
                    return;
                case Constants.UI.ButtonCommands.EDIT_TO_MAIN_MENU:
                    await _clientHelper.EditToMainMenu(ClientData);
                    await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.MainMenu);
                    return;
            }

            var userStage = await _userProvider.GetUserStage(ClientData.ChatId);

            if (!Enum.TryParse(typeof(ChatStageEnum), userStage?.Stage, out var chatStageEnum))
            {
                //TODO
                return;
            }

            var userStageEnum = (ChatStageEnum)chatStageEnum;

            switch (userStageEnum)
            {
                case ChatStageEnum.BirthLocationPicker:
                    if (ClientData?.Message?.Type == MessageType.Location &&
                        ClientData?.Message?.Location != null)
                    {
                        ClientData.Longitude = ClientData?.Message?.Location?.Longitude;
                        ClientData.Latitude = ClientData?.Message?.Location?.Latitude;

                        await _locationPicker.SendConfirmCoordinates(
                            ClientData,
                            $"{Constants.UI.Icons.Common.EARTH} {_localeManager.GetString("YourLocation", ClientData.CultureInfo)}\n" +
                            $"{_localeManager.GetString("Longitude", ClientData.CultureInfo)}: {ClientData.Longitude.Value.ToString("F6")}\n" +
                            $"{_localeManager.GetString("Latitude", ClientData.CultureInfo)}: {ClientData.Latitude.Value.ToString("F6")}\n");

                        await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.ConfirmBirthLocation);
                    }
                    else
                    {
                        //TODO
                    }
                    return;
            }

        }

        private async Task HandleCallbackAsync()
        {
            if (ClientData.CallbackData == null ||
                ClientData.CallbackData == Constants.UI.ButtonCommands.IGNORE)
            {
                return;
            }

            var userInfo = await _userProvider.GetUser(ClientData.ChatId);

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    _clientHelper.GetCancelButton(ClientData, _localeManager.GetString("ToMainMenu", ClientData.CultureInfo))
                }
            });

            var utcNowDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, 0, 0, DateTimeKind.Utc);

            switch (ClientData.CallbackData)
            {
                case Constants.UI.ButtonCommands.SEND_MAIN_MENU:
                    await _clientHelper.SendMainMenu(ClientData);
                    await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.MainMenu);
                    return;
                case Constants.UI.ButtonCommands.EDIT_TO_MAIN_MENU:
                    await _clientHelper.EditToMainMenu(ClientData);
                    await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.MainMenu);
                    return;
                case Constants.UI.ButtonCommands.NATAL_CHART:
                    if (userInfo == null ||
                        userInfo?.BirthDate == null ||
                        userInfo?.GmtOffset == null)
                    {
                        return;
                    }

                    var birthChart = await _calculationService.GetChartInfo(
                        userInfo.BirthDate.Value, 
                        userInfo.GmtOffset.Value, 
                        userInfo.Longitude.Value, 
                        userInfo.Latitude.Value,
                        userInfo.HouseSystem);

                    var natalPlanetsMessage = _clientHelper.GetNatalPlanetsMessage(birthChart, ClientData);

                    await _botClient.SendMessage(
                            chatId: ClientData.ChatId,
                            text: natalPlanetsMessage,
                            replyMarkup: null);

                    var housesMessage = _clientHelper.GetHousesMessage(birthChart, ClientData);

                    await _botClient.SendMessage(
                            chatId: ClientData.ChatId,
                            text: housesMessage,
                            replyMarkup: null);

                    var natalAspects = await _calculationService.GetNatalAspects(birthChart);

                    var natalMessages = _clientHelper.GetChartMessages(natalAspects, ClientData, ChartTypeEnum.Natal);

                    await _clientHelper.SendMessageHtml(ClientData.ChatId, natalMessages, keyboard);

                    return;
                case Constants.UI.ButtonCommands.TRANSIT_FORECAST:
                    if (userInfo == null ||
                        userInfo?.BirthDate == null ||
                        userInfo?.GmtOffset == null)
                    {
                        return;
                    }

                    await _botClient.SendMessage(
                            chatId: ClientData.ChatId,
                            text: $"{Constants.UI.Icons.Common.HOURGLASS} {_localeManager.GetString("ForecastProccessing", ClientData.CultureInfo)}",
                            replyMarkup: null);

                    var transitInterval = new TimeSpan(1, 0, 0, 0);

                    var aspects = await _calculationService.GetTransitAspects(
                        userInfo.BirthDate.Value,
                        userInfo.GmtOffset.Value,
                        utcNowDate,
                        transitInterval,
                        userInfo.Longitude.Value,
                        userInfo.Latitude.Value,
                        userInfo.HouseSystem);

                    var transitMessages = _clientHelper.GetChartMessages(aspects, ClientData, ChartTypeEnum.Transit);

                    await _clientHelper.SendMessageHtml(ClientData.ChatId, transitMessages, keyboard);

                    return;
                case Constants.UI.ButtonCommands.DIRECTION_FORECAST:
                    if (userInfo == null ||
                        userInfo?.BirthDate == null ||
                        userInfo?.GmtOffset == null)
                    {
                        return;
                    }

                    var directionAspects = await _calculationService.GetDirectionAspects(
                        userInfo.BirthDate.Value,
                        userInfo.GmtOffset.Value,
                        utcNowDate,
                        userInfo.Longitude.Value,
                        userInfo.Latitude.Value, 
                        userInfo.HouseSystem);

                    var directionMessages = _clientHelper.GetChartMessages(directionAspects, ClientData, ChartTypeEnum.Direction);

                    await _clientHelper.SendMessageHtml(ClientData.ChatId, directionMessages, keyboard);

                    return;
                case Constants.UI.ButtonCommands.POSITIVE_FORECAST:

                    return;
                case Constants.UI.ButtonCommands.SET_BIRTH_LOCATION:
                    await _locationPicker.SendLocation(
                        ClientData,
                        $"{Constants.UI.Icons.Common.EARTH}{_localeManager.GetString("ChooseBirthLocation", ClientData.CultureInfo)}");

                    await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.BirthLocationPicker);
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
                        case Constants.UI.ButtonCommands.SET_BIRTHDATE:
                            await _datePicker.SendYearIntervalPicker(
                                ClientData,
                                $"{_localeManager.GetString("ChooseBirthYear", ClientData.CultureInfo)}:");

                            await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.YearIntervalPicker);
                            break;
                    }
                    return;

                case ChatStageEnum.ConfirmBirthLocation:
                    if (ClientData.CallbackData.StartsWith(Constants.UI.ButtonCommands.SAVE_BIRTH_LOCATION) &&
                        _locationPicker.TryParseLocation(ClientData.CallbackData, out var longitude, out var latitude))
                    {
                        await _userProvider.EditLocation(ClientData.ChatId, longitude, latitude);

                        await _clientHelper.EditToMainMenu(ClientData);

                        await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.MainMenu);
                    }
                    else if (ClientData.CallbackData == Constants.UI.ButtonCommands.CHANGE_BIRTH_LOCATION)
                    {
                        await _locationPicker.SendLocation(
                            ClientData,
                            $"{Constants.UI.Icons.Common.EARTH}{_localeManager.GetString("ChooseBirthLocation", ClientData.CultureInfo)}");

                        await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.BirthLocationPicker);
                        return;
                    }
                    else if (ClientData.CallbackData == Constants.UI.ButtonCommands.CHANGE_BIRTH_LOCATION)
                    {
                        await _clientHelper.SendMainMenu(ClientData);
                        await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.MainMenu);
                    }

                    return;

                case ChatStageEnum.LanguagePicker:
                    if (Constants.UI.FlagsInfoDict.TryGetValue(ClientData.CallbackData, out (string Icon, string Descr) languageInfo))
                    {
                        ClientData.CultureInfo = new CultureInfo(ClientData.CallbackData);

                        await _userProvider.EditLanguage(ClientData.AstroUser.Id.Value, ClientData.CallbackData);
                    }

                    await _clientHelper.EditToMainMenu(ClientData);
                    await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.MainMenu);
                    return;

                case ChatStageEnum.HouseSystemPicker:
                    if (Enum.TryParse<HouseSystemEnum>(ClientData.CallbackData, out var houseSystem))
                    {
                        await _userProvider.EditHouseSystem(ClientData.AstroUser.Id.Value, houseSystem);
                    }

                    await _clientHelper.EditToMainMenu(ClientData);
                    await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.MainMenu);
                    return;

                case ChatStageEnum.YearIntervalPicker:
                    await _datePicker.SendYearPicker(
                        ClientData,
                        $"{_localeManager.GetString("ChooseBirthYear", ClientData.CultureInfo)}:");

                    await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.YearPicker);
                    return;

                case ChatStageEnum.YearPicker:
                    await _datePicker.SendMonthPicker(
                        ClientData,
                        $"{_localeManager.GetString("ChooseBirthMonth", ClientData.CultureInfo)}:");

                    await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.MonthPicker);
                    return;

                case ChatStageEnum.MonthPicker:
                    await _datePicker.SendDayPicker(
                        ClientData,
                        $"{_localeManager.GetString("ChooseBirthDay", ClientData.CultureInfo)}:\n" +
                        $"[ {ClientData.DatePickerData?.DateTime.Value.ToString("MMMM yyyy", ClientData.CultureInfo)} ]");

                    await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.DayPicker);
                    return;

                case ChatStageEnum.DayPicker:
                    await _datePicker.SendHourPicker(
                        ClientData,
                        $"{_localeManager.GetString("ChooseBirthHour", ClientData.CultureInfo)}:\n" +
                        $"[ {ClientData.DatePickerData?.DateTime.Value.ToString("d MMMM yyyy", ClientData.CultureInfo)} ]");

                    await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.HourPicker);
                    return;

                case ChatStageEnum.HourPicker:
                    await _datePicker.SendMinutePicker(
                        ClientData,
                        $"{_localeManager.GetString("ChooseBirthMinute", ClientData.CultureInfo)}:\n" +
                        $"[ {ClientData.DatePickerData?.DateTime.Value.ToString("d MMMM yyyy", ClientData.CultureInfo)} " +
                        $"{Constants.UI.Icons.Common.MINUS} " +
                        $"{ClientData.DatePickerData?.DateTime.Value.ToString("HH:XX", ClientData.CultureInfo)} ]");

                    await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.MinutePicker);
                    return;

                case ChatStageEnum.MinutePicker:
                    await _datePicker.SendTimeZonePicker(
                        ClientData,
                        $"{_localeManager.GetString("ChooseTimeZone", ClientData.CultureInfo)}: \n" +
                        $"[ {ClientData.DatePickerData?.DateTime.Value.ToString("d MMMM yyyy", ClientData.CultureInfo)} " +
                        $"{Constants.UI.Icons.Common.MINUS} " +
                        $"{ClientData.DatePickerData?.DateTime.Value.ToString("HH:mm", ClientData.CultureInfo)} ]");

                    await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.TimeZonePicker);
                    return;

                case ChatStageEnum.TimeZonePicker:
                    var gmtSign = ClientData.DatePickerData?.GmtOffset >= TimeSpan.Zero ? "+" : "-";
                    var gmtStr = ClientData.DatePickerData?.GmtOffset.Minutes == 0 ?
                        $"[GMT{gmtSign}{Math.Abs(ClientData.DatePickerData.GmtOffset.Hours)}]" :
                        $"[GMT{gmtSign}{Math.Abs(ClientData.DatePickerData.GmtOffset.Hours)}:{Math.Abs(ClientData.DatePickerData.GmtOffset.Minutes)}]";

                    await _datePicker.SendConfirmDate(
                        ClientData,
                        $"{Constants.UI.Icons.Common.SCIENCE} " +
                        $"{_localeManager.GetString("YourBirthDate", ClientData.CultureInfo)}:\n" +
                        $"{ClientData.DatePickerData?.DateTime.Value.ToString("d MMMM yyyy", ClientData.CultureInfo)} " +
                        $"{Constants.UI.Icons.Common.MINUS} " +
                        $"{ClientData.DatePickerData?.DateTime.Value.ToString("HH:mm", ClientData.CultureInfo)} " +
                        $"{gmtStr}");

                    await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.ConfirmBirthday);
                    return;

                case ChatStageEnum.ConfirmBirthday:
                    if (ClientData.DatePickerData?.IsSaveCommand ?? false)
                    {
                        await _userProvider.EditBirthday(ClientData.ChatId, ClientData.DatePickerData.DateTime, ClientData.DatePickerData.GmtOffset);

                        await _clientHelper.EditToMainMenu(ClientData);

                        await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.MainMenu);
                    }
                    else if (ClientData.DatePickerData?.IsChangeCommand ?? false)
                    {
                        await _datePicker.SendYearPicker(
                            ClientData,
                            $"{_localeManager.GetString("ChooseBirthYear", ClientData.CultureInfo)}:");

                        await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.YearPicker);
                    }
                    else if (ClientData.DatePickerData?.IsCancelCommand ?? false)
                    {
                        await _clientHelper.SendMainMenu(ClientData);
                        await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.MainMenu);
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
                var newAstroUser = new AstroUser()
                {
                    HouseSystem = HouseSystemEnum.Placidus
                };

                var currentCulture = CultureInfo.CurrentCulture;

                return new(newAstroUser, currentCulture);
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
                    Language = cultureInfo.Name,
                    HouseSystem = HouseSystemEnum.Placidus
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

            if (Constants.UI.LocaleDict.TryGetValue(language, out var cultureInfo))
            {
                return cultureInfo;
            }

            return new CultureInfo("en-US");
        }

        private async Task SetLanguage()
        {
            await _clientHelper.SendLanguagePicker(ClientData);
            await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.LanguagePicker);
        }

        private async Task SetHouseSystem()
        {
            await _clientHelper.SendHouseSystemPicker(ClientData);
            await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.HouseSystemPicker);
        }

        private async Task SetBirthday()
        {
            await _datePicker.SendYearIntervalPicker(
                ClientData,
                $"{_localeManager.GetString("ChooseBirthYear", ClientData.CultureInfo)}:");

            await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.YearIntervalPicker);
        }

        private async Task SetLocation()
        {
            await _locationPicker.SendLocation(
                ClientData,
                $"{Constants.UI.Icons.Common.EARTH}{_localeManager.GetString("ChooseBirthLocation", ClientData.CultureInfo)}");

            await _userProvider.SetUserStage(ClientData.ChatId, ChatStageEnum.BirthLocationPicker);
        }
    }
}
