using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Polling;
using AstroTBotService.Enums;
using AstroTBotService.Entities;
using AstroTBotService.Db.Providers;
using AstroTBotService.Db.Entities;
using AstroTBotService.AstroCalculation.Services;
using Telegram.Bot.Types.ReplyMarkups;
using AstroTBotService.Redis;

namespace AstroTBotService.TBot
{
    public class TBotUpdateHandler(
        ITelegramBotClient botClient,
        ITClientHelper clientHelper,
        IPersonDataPicker personDataPicker,
        IUserProvider userProvider,
        ICalculationService calculationService,
        IResourcesLocaleManager localeManager,
        IRedisService redisService,
        ILogger<TBotUpdateHandler> logger) : IUpdateHandler
    {
        private readonly ITelegramBotClient _botClient = botClient;
        private readonly ITClientHelper _clientHelper = clientHelper;
        private readonly IPersonDataPicker _personDataPicker = personDataPicker;
        private readonly IUserProvider _userProvider = userProvider;
        private readonly IResourcesLocaleManager _localeManager = localeManager;
        private readonly IRedisService _redisService = redisService;
        private readonly ICalculationService _calculationService = calculationService;
        private readonly ILogger<TBotUpdateHandler> _logger = logger;

        public TBotClientData ClientData { get; private set; }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var user = new AstroUser();
            var userStage = new UserStage();

            var stage = ChatStageEnum.Menu;
            RedisPersonData? personData;

            try
            {
                user = await GetOrCreateUser(update);
                userStage = await _userProvider.GetUserStage(user.Id.Value);

                if (userStage == null)
                {
                    await _userProvider.SetUserStage(user.Id.Value, ChatStageEnum.Menu);
                }

                if (!Enum.TryParse(typeof(ChatStageEnum), userStage?.Stage, out var userStageObj))
                {
                    _logger.LogError($"Wrong chat stage: {userStage?.Stage?.ToString()}. Set Menu stage.");

                    await _userProvider.SetUserStage(user.Id.Value, ChatStageEnum.Menu);
                    stage = ChatStageEnum.Menu;

                    return;
                }

                stage = (ChatStageEnum)userStageObj;

                personData = await _redisService.GetPersonData(user.Id.Value);
            }
            catch
            {
                throw;
            }

            ClientData = new TBotClientData()
            {
                AstroUser = user,
                Message = update?.Message ?? update?.CallbackQuery?.Message,
                CallbackData = update?.CallbackQuery?.Data,
                ChatStageEnum = stage,
                RedisPersonData = personData
            };

            _logger.LogInformation($"Begin processing response. User id: {ClientData.AstroUserId}. User stage: {ClientData.ChatStageEnum}.");

            if (update?.Message != null)
            {
                await HandleMessageAsync();
            }
            else if (update?.CallbackQuery != null)
            {
                await HandleCallbackAsync();
            }
            else
            {
                _logger.LogError($"Message and callback is null");
                return;
            }
        }

        private async Task HandleMessageAsync()
        {
            _logger.LogInformation("Message handling");

            if (ClientData.Message.Type != MessageType.Text
                && ClientData.Message.Type != MessageType.Location
                && ClientData.Message.Type != MessageType.Venue)
            {
                _logger.LogWarning($"Wrong message type error: {ClientData.Message.Type}");
                return;
            }

            if (await HandleMenuCommandsAsync())
            {
                _logger.LogInformation($"Handled menu command: \"{ClientData.Message.Text?.ToLower()}\"");

                return;
            }

            switch (ClientData.ChatStageEnum)
            {
                case ChatStageEnum.BirthLocationPicker:
                    if ((ClientData.Message.Type == MessageType.Location || ClientData.Message.Type == MessageType.Venue)
                        && ClientData.Message.Location != null)
                    {
                        await _redisService.SetPersonLongitude(ClientData.AstroUserId, ClientData.Message.Location.Longitude);
                        await _redisService.SetPersonLatitude(ClientData.AstroUserId, ClientData.Message.Location.Latitude);

                        ClientData.RedisPersonData.Longitude = ClientData.Message.Location?.Longitude;
                        ClientData.RedisPersonData.Latitude = ClientData.Message.Location?.Latitude;

                        var messageText = GetPersonInfoText(Constants.UI.Icons.Common.MOON_FACE);

                        await _personDataPicker.SendConfirmDate(
                            ClientData,
                            messageText);

                        await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.ConfirmBirthday);
                    }
                    else
                    {
                        _logger.LogWarning($"Wrong message type for stage {ClientData.ChatStageEnum}.");
                    }
                    return;
                case ChatStageEnum.PersonNamePicker:
                    var personName = ClientData.Message.Text;

                    var personNamePickerButtons = new InlineKeyboardMarkup(new[]
                    {
                            new []
                            {
                                _clientHelper.GetBackToPersonsButton(ClientData),
                                _clientHelper.GetCancelButton(ClientData, _localeManager.GetString("ToMenu", ClientData.AstroUser.CultureInfo))
                            }
                        });

                    if (string.IsNullOrWhiteSpace(personName)
                        || personName.Length == 0
                        || personName.Length > 16)
                    {
                        await _clientHelper.SendMessage(
                            ClientData.AstroUserId,
                            $"{_localeManager.GetString("PersonNameLengthError", ClientData.AstroUser.CultureInfo)}.",
                            personNamePickerButtons);

                        return;
                    }

                    //Edit person
                    if (string.IsNullOrWhiteSpace(ClientData.RedisPersonData.EditingPersonType))
                    {
                        await _redisService.CreatePersonData(ClientData.AstroUserId);
                    }
                    //New person
                    else if (ClientData.RedisPersonData.EditingPersonType == Constants.UI.Buttons.PersonTypes.PERSON)
                    {
                        await _redisService.CreatePersonDataForEdit(ClientData.AstroUserId, ClientData.RedisPersonData.EditingPersonId);
                    }
                    //Wrong redis value
                    else
                    {
                        _logger.LogError($"Wrong Redis {nameof(ClientData.RedisPersonData.EditingPersonType)}: {ClientData.RedisPersonData.EditingPersonType}");
                    }

                    await _redisService.SetPersonName(ClientData.AstroUserId, personName);
                    ClientData.RedisPersonData.Name = personName;

                    var message = GetPersonInfoText(Constants.UI.Icons.Common.MOON_2);

                    message += ClientData.RedisPersonData.IsUser
                        ? $" {_localeManager.GetString("ChooseYourBirthYear", ClientData.AstroUser.CultureInfo)}:"
                        : $"\n\n{Constants.UI.Icons.Common.BLUE_SMALL_RHOMB}{_localeManager.GetString("ChooseBirthYear", ClientData.AstroUser.CultureInfo)}:";

                    await _personDataPicker.SendYearIntervalPicker(
                        ClientData,
                        message);

                    await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.YearIntervalPicker);
                    break;
            }
        }

        private async Task HandleCallbackAsync()
        {
            _logger.LogInformation("Callback handling.");


            if (ClientData.CallbackData == null ||
                ClientData.CallbackData == Constants.UI.Buttons.Commands.IGNORE)
            {
                return;
            }

            switch (ClientData.ChatStageEnum)
            {
                case ChatStageEnum.Menu:
                    await HandleCallbackButtons(
                        Constants.UI.Buttons.Commands.SEND_MENU,
                        Constants.UI.Buttons.Commands.EDIT_TO_MENU,
                        Constants.UI.Buttons.Commands.ADD_USER,
                        Constants.UI.Buttons.Commands.SET_BIRTH_LOCATION,
                        Constants.UI.Buttons.Commands.NATAL_CHART,
                        Constants.UI.Buttons.Commands.TRANSIT_DAILY_FORECAST,
                        Constants.UI.Buttons.Commands.DIRECTION_FORECAST,
                        Constants.UI.Buttons.Commands.GET_PERSONS);
                    return;
                case ChatStageEnum.Persons:
                    if (await HandleCallbackButtons(
                        Constants.UI.Buttons.Commands.SEND_MENU,
                        Constants.UI.Buttons.Commands.EDIT_TO_MENU,
                        Constants.UI.Buttons.Commands.GET_PERSONS,
                        Constants.UI.Buttons.Commands.ADD_PERSON,
                        Constants.UI.Buttons.Commands.EDIT_PERSON,
                        Constants.UI.Buttons.Commands.DELETE_PERSON))
                    {
                        return;
                    }

                    await HandlePersonsButtons();

                    _logger.LogInformation("Enter person name message is sent");
                    return;
                case ChatStageEnum.PersonNamePicker:
                    await HandleCallbackButtons(
                        Constants.UI.Buttons.Commands.SEND_MENU,
                        Constants.UI.Buttons.Commands.EDIT_TO_MENU,
                        Constants.UI.Buttons.Commands.GET_PERSONS);
                    return;
                case ChatStageEnum.YearIntervalPicker:
                    if (await HandleCallbackButtons(
                        Constants.UI.Buttons.Commands.SEND_MENU,
                        Constants.UI.Buttons.Commands.EDIT_TO_MENU))
                    {
                        return;
                    }

                    if (!int.TryParse(ClientData.CallbackData, out var startYear))
                    {
                        _logger.LogError($"Wrong callback data for stage \"{ClientData.ChatStageEnum}\": {ClientData.CallbackData}");
                        return;
                    }

                    await _redisService.SetPersonYearInterval(ClientData.AstroUserId, startYear);
                    ClientData.RedisPersonData.StartYearInterval = startYear;

                    var yearPickerText = GetPersonInfoText(Constants.UI.Icons.Common.MOON_2);

                    yearPickerText += ClientData.RedisPersonData.IsUser
                        ? $" {_localeManager.GetString("ChooseYourBirthYear", ClientData.AstroUser.CultureInfo)}"
                        : $"\n\n{Constants.UI.Icons.Common.BLUE_SMALL_RHOMB}{_localeManager.GetString("ChooseBirthYear", ClientData.AstroUser.CultureInfo)}";

                    await _personDataPicker.SendYearPicker(
                        ClientData,
                        $"{yearPickerText}:");

                    await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.YearPicker);
                    return;
                case ChatStageEnum.YearPicker:
                    if (await HandleCallbackButtons(
                        Constants.UI.Buttons.Commands.SEND_MENU,
                        Constants.UI.Buttons.Commands.EDIT_TO_MENU))
                    {
                        return;
                    }

                    if (!int.TryParse(ClientData.CallbackData, out var year))
                    {
                        _logger.LogError($"Wrong callback data for stage \"{ClientData.ChatStageEnum}\": {ClientData.CallbackData}");
                        return;
                    }

                    await _redisService.SetPersonYear(ClientData.AstroUserId, year);
                    ClientData.RedisPersonData.Year = year;

                    var monthPickerText = GetPersonInfoText(Constants.UI.Icons.Common.MOON_3);

                    monthPickerText += ClientData.RedisPersonData.IsUser
                        ? $" {_localeManager.GetString("ChooseYourBirthMonth", ClientData.AstroUser.CultureInfo)}"
                        : $"\n\n{Constants.UI.Icons.Common.BLUE_SMALL_RHOMB}{_localeManager.GetString("ChooseBirthMonth", ClientData.AstroUser.CultureInfo)}";

                    await _personDataPicker.SendMonthPicker(
                        ClientData,
                        $"{monthPickerText}:");

                    await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.MonthPicker);
                    return;
                case ChatStageEnum.MonthPicker:
                    if (await HandleCallbackButtons(
                        Constants.UI.Buttons.Commands.SEND_MENU,
                        Constants.UI.Buttons.Commands.EDIT_TO_MENU))
                    {
                        return;
                    }

                    if (!int.TryParse(ClientData.CallbackData, out var month))
                    {
                        _logger.LogError($"Wrong callback data for stage \"{ClientData.ChatStageEnum}\": {ClientData.CallbackData}");
                        return;
                    }
                    await _redisService.SetPersonMonth(ClientData.AstroUserId, month);
                    ClientData.RedisPersonData.Month = month;

                    var dayPickerText = GetPersonInfoText(Constants.UI.Icons.Common.MOON_4);

                    dayPickerText += ClientData.RedisPersonData.IsUser
                        ? $" {_localeManager.GetString("ChooseYourBirthDay", ClientData.AstroUser.CultureInfo)}"
                        : $"\n\n{Constants.UI.Icons.Common.BLUE_SMALL_RHOMB}{_localeManager.GetString("ChooseBirthDay", ClientData.AstroUser.CultureInfo)}";

                    await _personDataPicker.SendDayPicker(
                        ClientData,
                        $"{dayPickerText}:\n");

                    await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.DayPicker);
                    return;
                case ChatStageEnum.DayPicker:
                    if (await HandleCallbackButtons(
                        Constants.UI.Buttons.Commands.SEND_MENU,
                        Constants.UI.Buttons.Commands.EDIT_TO_MENU))
                    {
                        return;
                    }

                    if (!int.TryParse(ClientData.CallbackData, out var day))
                    {
                        _logger.LogError($"Wrong callback data for stage \"{ClientData.ChatStageEnum}\": {ClientData.CallbackData}");
                        return;
                    }

                    await _redisService.SetPersonDay(ClientData.AstroUserId, day);
                    ClientData.RedisPersonData.Day = day;

                    var hourPickerText = GetPersonInfoText(Constants.UI.Icons.Common.MOON_5);

                    hourPickerText += ClientData.RedisPersonData.IsUser
                        ? $"\n\n{_localeManager.GetString("ChooseYourBirthHour", ClientData.AstroUser.CultureInfo)}"
                        : $"\n\n{Constants.UI.Icons.Common.BLUE_SMALL_RHOMB}{_localeManager.GetString("ChooseBirthHour", ClientData.AstroUser.CultureInfo)}";

                    await _personDataPicker.SendHourPicker(
                        ClientData,
                        $"{hourPickerText}:\n");

                    await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.HourPicker);
                    return;
                case ChatStageEnum.HourPicker:
                    if (await HandleCallbackButtons(
                        Constants.UI.Buttons.Commands.SEND_MENU,
                        Constants.UI.Buttons.Commands.EDIT_TO_MENU))
                    {
                        return;
                    }

                    if (!int.TryParse(ClientData.CallbackData, out var hour))
                    {
                        _logger.LogError($"Wrong callback data for stage \"{ClientData.ChatStageEnum}\": {ClientData.CallbackData}");
                        return;
                    }

                    await _redisService.SetPersonHour(ClientData.AstroUserId, hour);
                    ClientData.RedisPersonData.Hour = hour;

                    var minutePickerText = GetPersonInfoText(Constants.UI.Icons.Common.MOON_6);

                    minutePickerText += ClientData.RedisPersonData.IsUser
                        ? $"\n\n{_localeManager.GetString("ChooseYourBirthMinute", ClientData.AstroUser.CultureInfo)}"
                        : $"\n\n{Constants.UI.Icons.Common.BLUE_SMALL_RHOMB}{_localeManager.GetString("ChooseBirthMinute", ClientData.AstroUser.CultureInfo)}";

                    await _personDataPicker.SendMinutePicker(
                        ClientData,
                        $"{minutePickerText}:");

                    await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.MinutePicker);
                    return;
                case ChatStageEnum.MinutePicker:
                    if (await HandleCallbackButtons(
                        Constants.UI.Buttons.Commands.SEND_MENU,
                        Constants.UI.Buttons.Commands.EDIT_TO_MENU))
                    {
                        return;
                    }

                    if (!int.TryParse(ClientData.CallbackData, out var minute))
                    {
                        _logger.LogError($"Wrong callback data for stage \"{ClientData.ChatStageEnum}\": {ClientData.CallbackData}");
                        return;
                    }

                    await _redisService.SetPersonMinute(ClientData.AstroUserId, minute);
                    ClientData.RedisPersonData.Minute = minute;

                    var timeZonePickerText = GetPersonInfoText(Constants.UI.Icons.Common.MOON_7);

                    timeZonePickerText += ClientData.RedisPersonData.IsUser
                        ? $"\n\n{_localeManager.GetString("ChooseYourTimeZone", ClientData.AstroUser.CultureInfo)}"
                        : $"\n\n{Constants.UI.Icons.Common.BLUE_SMALL_RHOMB}{_localeManager.GetString("ChooseTimeZone", ClientData.AstroUser.CultureInfo)}";

                    await _personDataPicker.SendTimeZonePicker(
                        ClientData,
                        $"{timeZonePickerText}:");

                    await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.TimeZonePicker);
                    return;
                case ChatStageEnum.TimeZonePicker:
                    if (await HandleCallbackButtons(
                        Constants.UI.Buttons.Commands.SEND_MENU,
                        Constants.UI.Buttons.Commands.EDIT_TO_MENU))
                    {
                        return;
                    }

                    if (!TimeSpan.TryParse(ClientData.CallbackData, out var timeZone))
                    {
                        _logger.LogError($"Wrong callback data for stage \"{ClientData.ChatStageEnum}\": {ClientData.CallbackData}");
                        return;
                    }

                    await _redisService.SetPersonTimeZone(ClientData.AstroUserId, timeZone);
                    ClientData.RedisPersonData.TimeZoneMilliseconds = timeZone.TotalMilliseconds;

                    var locationPickerText = GetPersonInfoText(Constants.UI.Icons.Common.MOON_8);

                    locationPickerText += ClientData.RedisPersonData.IsUser
                        ? $"\n\n{_localeManager.GetString("ChooseYourBirthLocation", ClientData.AstroUser.CultureInfo)}"
                        : $"\n\n{Constants.UI.Icons.Common.BLUE_SMALL_RHOMB}{_localeManager.GetString("ChooseBirthLocation", ClientData.AstroUser.CultureInfo)}";

                    await SendLocation(
                        ClientData,
                        locationPickerText);

                    await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.BirthLocationPicker);
                    return;
                case ChatStageEnum.ConfirmBirthday:
                    await HandleCallbackButtons(
                        Constants.UI.Buttons.Commands.SEND_MENU,
                        Constants.UI.Buttons.Commands.EDIT_TO_MENU,
                        Constants.UI.Buttons.Commands.SAVE_BIRTHDAY,
                        Constants.UI.Buttons.Commands.CHANGE_BIRTHDAY,
                        Constants.UI.Buttons.Commands.CHANGE_BIRTH_LOCATION);

                    return;
                case ChatStageEnum.BirthLocationPicker:
                    await HandleCallbackButtons(
                        Constants.UI.Buttons.Commands.SEND_MENU,
                        Constants.UI.Buttons.Commands.EDIT_TO_MENU);
                    return;
                case ChatStageEnum.LanguagePicker:
                    if (await HandleCallbackButtons(
                        Constants.UI.Buttons.Commands.SEND_MENU,
                        Constants.UI.Buttons.Commands.EDIT_TO_MENU))
                    {
                        return;
                    }

                    await _userProvider.EditLanguage(ClientData.AstroUserId, ClientData.CallbackData);
                    ClientData.AstroUser.Language = ClientData.CallbackData;

                    await _clientHelper.EditToMenu(ClientData);
                    await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.Menu);
                    return;
                case ChatStageEnum.HouseSystemPicker:
                    if (await HandleCallbackButtons(
                        Constants.UI.Buttons.Commands.SEND_MENU,
                        Constants.UI.Buttons.Commands.EDIT_TO_MENU))
                    {
                        return;
                    }

                    if (int.TryParse(ClientData.CallbackData, out int stageNum)
                        && Enum.IsDefined(typeof(HouseSystemEnum), stageNum)
                        && ClientData.AstroUser.HouseSystem != (HouseSystemEnum)stageNum)
                    {
                        await _userProvider.EditHouseSystem(ClientData.AstroUserId, (HouseSystemEnum)stageNum);
                    }
                    else
                    {
                        _logger.LogError($"Wrong callback data for stage \"{ClientData.ChatStageEnum}\": {ClientData.CallbackData}");
                    }

                    await _clientHelper.EditToMenu(ClientData);
                    await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.Menu);
                    return;
            }
        }

        private string GetPersonInfoText(string birthIcon)
        {
            var oldPersonText = string.Empty;

            //UserInfo
            if (ClientData.RedisPersonData.EditingPersonType == Constants.UI.Buttons.PersonTypes.USER)
            {
                var oldUserInfo = ClientData.AstroUser;

                if (oldUserInfo != null)
                {
                    oldPersonText = $"{Constants.UI.Icons.Common.MOON_BLACK_FACE} {_localeManager.GetString("You", oldUserInfo.CultureInfo)} " +
                        $"[{_localeManager.GetString("OldPersonDataForEditing", oldUserInfo.CultureInfo)}]\n" +
                        $"{_localeManager.GetString("BirthDate", oldUserInfo.CultureInfo)}:\n" +
                        $"{oldUserInfo.DateToLongString(oldUserInfo.CultureInfo)}";

                    if (oldUserInfo.Longitude.HasValue
                        && oldUserInfo.Latitude.HasValue)
                    {
                        oldPersonText += $"\n\n{_localeManager.GetString("Location", oldUserInfo.CultureInfo)}:\n" +
                            $"{_localeManager.GetString("Longitude", oldUserInfo.CultureInfo)}: {oldUserInfo.Longitude.Value.ToString("F6")}\n" +
                            $"{_localeManager.GetString("Latitude", oldUserInfo.CultureInfo)}: {oldUserInfo.Latitude.Value.ToString("F6")}\n\n";
                    }
                }
            }
            //PersonInfo
            else if (ClientData.RedisPersonData.EditingPersonType == Constants.UI.Buttons.PersonTypes.PERSON)
            {
                var oldPersonInfo = ClientData.AstroUser.ChildPersons.FirstOrDefault(p => p.Id == ClientData.RedisPersonData.EditingPersonId);

                if (oldPersonInfo != null)
                {
                    oldPersonText = $"{Constants.UI.Icons.Common.MOON_BLACK_FACE} {oldPersonInfo.Name} " +
                        $"[{_localeManager.GetString("OldPersonDataForEditing", ClientData.AstroUser.CultureInfo)}]\n" +
                        $"{_localeManager.GetString("BirthDate", ClientData.AstroUser.CultureInfo)}:\n" +
                        $"{oldPersonInfo.DateToLongString(ClientData.AstroUser.CultureInfo)}";

                    if (oldPersonInfo.Longitude.HasValue
                        && oldPersonInfo.Latitude.HasValue)
                    {
                        oldPersonText += $"\n\n{_localeManager.GetString("Location", ClientData.AstroUser.CultureInfo)}:\n" +
                        $"{_localeManager.GetString("Longitude", ClientData.AstroUser.CultureInfo)}: {oldPersonInfo.Longitude.Value.ToString("F6")}\n" +
                        $"{_localeManager.GetString("Latitude", ClientData.AstroUser.CultureInfo)}: {oldPersonInfo.Latitude.Value.ToString("F6")}\n\n";
                    }
                }
            }
            //Wrong redis value
            else
            {
                var errorMessage = $"Wrong Redis {nameof(ClientData.RedisPersonData.EditingPersonType)}: {ClientData.RedisPersonData.EditingPersonType}";
                _logger.LogError(errorMessage);

                throw new ArgumentException(errorMessage);
            }

            //New Data
            var birthDate = string.Empty;
            var birthTime = string.Empty;
            var birthMessage = birthIcon;

            if (!string.IsNullOrWhiteSpace(ClientData.RedisPersonData.Name)
                && !ClientData.RedisPersonData.IsUser)
            {
                birthMessage += $" {ClientData.RedisPersonData.Name}";
            }

            if (ClientData.RedisPersonData.Year != 0
                && ClientData.RedisPersonData.Month != 0
                && ClientData.RedisPersonData.Day != 0)
            {
                birthMessage += ClientData.RedisPersonData.IsUser
                    ? $" {_localeManager.GetString("YourBirthDate", ClientData.AstroUser.CultureInfo)}:\n"
                    : $"\n{_localeManager.GetString("BirthDate", ClientData.AstroUser.CultureInfo)}:\n";

                birthDate = ClientData.RedisPersonData?.GetDateTime().ToString("d MMMM yyyy", ClientData.AstroUser.CultureInfo);

                birthMessage += $"{birthDate}";

                if (ClientData.RedisPersonData.Hour != 0
                    && ClientData.RedisPersonData.Minute != 0)
                {
                    birthTime = ClientData.RedisPersonData?.GetDateTime().ToString("HH:mm", ClientData.AstroUser.CultureInfo);
                    birthMessage += $" {Constants.UI.Icons.Common.MINUS} {birthTime}";
                }
            }

            if (ClientData.RedisPersonData.TimeZoneMilliseconds.HasValue)
            {
                var gmtSign = ClientData.RedisPersonData?.TimeZoneMilliseconds >= 0 ? "+" : "-";
                var gmtStr = ClientData.RedisPersonData?.TimeZoneMilliseconds % 3_600_000 == 0 ?
                    $"[GMT{gmtSign}{Math.Abs(ClientData.RedisPersonData.GetTimeZone().Hours)}]" :
                    $"[GMT{gmtSign}{Math.Abs(ClientData.RedisPersonData.GetTimeZone().Hours)}:{Math.Abs(ClientData.RedisPersonData.GetTimeZone().Minutes)}]";

                birthMessage += $" {gmtStr}";
            }

            //New location
            var locationStr = string.Empty;
            var longitudeStr = string.Empty;
            var latitudeStr = string.Empty;

            if (ClientData.RedisPersonData.Longitude.HasValue
                && ClientData.RedisPersonData.Latitude.HasValue)
            {
                locationStr += "\n\n";

                longitudeStr = ClientData.RedisPersonData?.Longitude.Value.ToString("F6");
                latitudeStr = ClientData.RedisPersonData?.Latitude.Value.ToString("F6");

                locationStr += $"{Constants.UI.Icons.Common.EARTH} {_localeManager.GetString("Location", ClientData.AstroUser.CultureInfo)}:\n" +
                    $"{_localeManager.GetString("Longitude", ClientData.AstroUser.CultureInfo)}: {longitudeStr}\n" +
                    $"{_localeManager.GetString("Latitude", ClientData.AstroUser.CultureInfo)}: {latitudeStr}";
            }

            var messageText = $"{oldPersonText}{birthMessage}{locationStr}";

            return messageText;
        }

        private async Task<bool> HandleMenuCommandsAsync()
        {
            if (ClientData.Message.Text?.ToLower() == Constants.UI.MessageCommands.START)
            {
                await _redisService.DeletePersonData(ClientData.AstroUserId);

                await _clientHelper.SendMenu(ClientData);
                await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.Menu);

                return true;
            }
            else if (ClientData.Message.Text?.ToLower() == Constants.UI.MessageCommands.SET_BIRTHDATE)
            {
                await _redisService.DeletePersonData(ClientData.AstroUserId);

                await SetBirthday();
                return true;
            }
            else if (ClientData.Message.Text?.ToLower() == Constants.UI.MessageCommands.SET_LANGUAGE)
            {
                await _redisService.DeletePersonData(ClientData.AstroUserId);

                await SetLanguage();
                return true;
            }
            else if (ClientData.Message.Text?.ToLower() == Constants.UI.MessageCommands.SET_HOUSES_SYSTEM)
            {
                await _redisService.DeletePersonData(ClientData.AstroUserId);

                await SetHouseSystem();
                return true;
            }

            return false;
        }

        private async Task BeginEditingUser(bool isSendNewMessage)
        {
            await _redisService.CreateUserData(ClientData.AstroUserId);
            await _redisService.SetEditingUserId(ClientData.AstroUserId);

            ClientData.RedisPersonData.EditingPersonId = ClientData.AstroUserId;
            ClientData.RedisPersonData.EditingPersonType = Constants.UI.Buttons.PersonTypes.USER;

            var oldPersonText = GetPersonInfoText(Constants.UI.Icons.Common.MOON_2);

            if (isSendNewMessage)
            {
                await _personDataPicker.SendYearIntervalPicker(
                    ClientData,
                    $"{oldPersonText} {_localeManager.GetString("ChooseYourBirthYear", ClientData.AstroUser.CultureInfo)}:");
            }
            else
            {
                await _personDataPicker.EditToYearIntervalPicker(
                    ClientData,
                    $"{oldPersonText} {_localeManager.GetString("ChooseYourBirthYear", ClientData.AstroUser.CultureInfo)}:");
            }
        }

        private async Task HandlePersonsButtons()
        {
            var buttonInfo = ClientData.CallbackData.Split(Constants.UI.Buttons.SEPARATOR);

            if (buttonInfo.Length != 3)
            {
                _logger.LogError($"Wrong button callback data for stage \"{ClientData.ChatStageEnum}\": {ClientData.CallbackData}. Expected 3 parts with separator: \"{Constants.UI.Buttons.SEPARATOR}\"");
                return;
            }

            var userPrefix = buttonInfo[0];
            var commandPrefix = buttonInfo[1];
            var idString = buttonInfo[2];

            if (userPrefix != Constants.UI.Buttons.PersonTypes.USER &&
                userPrefix != Constants.UI.Buttons.PersonTypes.PERSON)
            {
                _logger.LogError($"Wrong button callback data for stage \"{ClientData.ChatStageEnum}\": {ClientData.CallbackData}. " +
                    $"Expected \"{Constants.UI.Buttons.PersonTypes.USER}\" or \"{Constants.UI.Buttons.PersonTypes.PERSON}\" in the first part of callback");
                return;
            }

            if (commandPrefix != Constants.UI.Buttons.CommandTypes.GET &&
                commandPrefix != Constants.UI.Buttons.CommandTypes.EDIT &&
                commandPrefix != Constants.UI.Buttons.CommandTypes.DELETE)
            {
                _logger.LogError($"Wrong button callback data for stage \"{ClientData.ChatStageEnum}\": {ClientData.CallbackData}. " +
                    $"Expected \"{Constants.UI.Buttons.CommandTypes.GET}\" or \"{Constants.UI.Buttons.CommandTypes.EDIT}\" or \"{Constants.UI.Buttons.CommandTypes.DELETE}\" in the second part of callback");
                return;
            }

            if (!long.TryParse(idString, out var id))
            {
                _logger.LogError($"Wrong button callback data for stage \"{ClientData.ChatStageEnum}\": {ClientData.CallbackData}. " +
                    $"Expected number (\"long\" type) in the third part of callback");
                return;
            }

            switch (commandPrefix)
            {
                case Constants.UI.Buttons.CommandTypes.GET:
                    var choosePersons = ClientData.AstroUser.GetAllPersons();
                    await ChangePerson(choosePersons, id, userPrefix == Constants.UI.Buttons.PersonTypes.USER);

                    await _clientHelper.EditToMenu(ClientData);
                    await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.Menu);

                    return;
                case Constants.UI.Buttons.CommandTypes.EDIT:
                    if (userPrefix == Constants.UI.Buttons.PersonTypes.USER)
                    {
                        await BeginEditingUser(false);

                        await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.YearIntervalPicker);
                    }
                    else if (userPrefix == Constants.UI.Buttons.PersonTypes.PERSON)
                    {
                        await _redisService.CreatePersonDataForEdit(ClientData.AstroUserId, id);

                        var editPersonKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new []
                            {
                                _clientHelper.GetBackToPersonsButton(ClientData),
                                _clientHelper.GetCancelButtonWithEdit(ClientData)
                            }
                        });

                        await _redisService.SetEditingPersonId(ClientData.AstroUserId, id);
                        ClientData.RedisPersonData.EditingPersonId = id;
                        ClientData.RedisPersonData.EditingPersonType = userPrefix;

                        var oldPersonText = GetPersonInfoText(Constants.UI.Icons.Common.MOON_1);

                        await _clientHelper.EditMessage(
                            ClientData.AstroUserId,
                            ClientData.Message.Id,
                            $"{oldPersonText} {_localeManager.GetString("EnterPersonName", ClientData.AstroUser.CultureInfo)}:",
                            editPersonKeyboard);

                        await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.PersonNamePicker);
                    }

                    return;
                case Constants.UI.Buttons.CommandTypes.DELETE:
                    var person = ClientData.AstroUser.ChildPersons
                        .FirstOrDefault(p => p.Id == id);

                    if (person == null)
                    {
                        _logger.LogError($"Wrong button callback data for stage \"{ClientData.ChatStageEnum}\": {ClientData.CallbackData}. " +
                            $"Can't find person with id = {id}");
                        return;
                    }

                    if (person.IsChosen == true)
                    {
                        ClientData.AstroUser.IsChosen = true;
                        person.IsChosen = false;
                    }

                    await _userProvider.DeletePerson(person);

                    await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.Persons);
                    await _clientHelper.SendPersons(ClientData, Constants.UI.Buttons.CommandTypes.GET);
                    return;
            }

            await _clientHelper.SendPersons(ClientData, commandPrefix);
        }

        private async Task<bool> HandleCallbackButtons(params string[] allowedCommand)
        {
            if (!allowedCommand.Contains(ClientData.CallbackData))
            {
                return false;
            }

            var utcNowDate = new DateTime(
                DateTime.UtcNow.Year,
                DateTime.UtcNow.Month,
                DateTime.UtcNow.Day,
                DateTime.UtcNow.Hour,
                0,
                0,
                DateTimeKind.Utc);

            var cancelKeyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    _clientHelper.GetCancelButton(ClientData, _localeManager.GetString("ToMenu", ClientData.AstroUser.CultureInfo))
                }
            });

            switch (ClientData.CallbackData)
            {
                case Constants.UI.Buttons.Commands.SEND_MENU:
                    _logger.LogInformation("Send main menu button handling.");

                    await _redisService.DeletePersonData(ClientData.AstroUserId);

                    await _clientHelper.SendMenu(ClientData);
                    await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.Menu);

                    return true;
                case Constants.UI.Buttons.Commands.EDIT_TO_MENU:
                    _logger.LogInformation("Edit to main menu button handling.");

                    await _redisService.DeletePersonData(ClientData.AstroUserId);

                    await _clientHelper.EditToMenu(ClientData);
                    await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.Menu);

                    return true;
                case Constants.UI.Buttons.Commands.GET_PERSONS:
                    _logger.LogInformation("Persons button handling.");

                    if (ClientData.AstroUser == null ||
                        ClientData.AstroUser?.BirthDate == null ||
                        ClientData.AstroUser?.TimeZoneOffset == null)
                    {
                        _logger.LogError("Empty user info error.");

                        return true;
                    }

                    _logger.LogInformation("Getting persons.");

                    await _clientHelper.SendPersons(ClientData, Constants.UI.Buttons.CommandTypes.GET);
                    await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.Persons);

                    _logger.LogInformation("Getting persons message is sent.");

                    return true;
                case Constants.UI.Buttons.Commands.ADD_PERSON:
                    _logger.LogInformation("Add person button handling.");

                    if (ClientData.AstroUser == null ||
                        ClientData.AstroUser?.BirthDate == null ||
                        ClientData.AstroUser?.TimeZoneOffset == null)
                    {
                        _logger.LogError("Empty user info error.");

                        return true;
                    }

                    if (ClientData.AstroUser.ChildPersons.Count >= 3)
                    {
                        var addPersonWarningKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new []
                            {
                                _clientHelper.GetBackToPersonsButton(ClientData),
                                _clientHelper.GetCancelButton(ClientData, _localeManager.GetString("ToMenu", ClientData.AstroUser.CultureInfo))
                            }
                        });

                        await _clientHelper.EditMessage(
                            ClientData.AstroUserId,
                            ClientData.Message.Id,
                            $"{Constants.UI.Icons.Common.WARNING_RED} {_localeManager.GetString("MaxPersonsCountError", ClientData.AstroUser.CultureInfo)}.",
                            addPersonWarningKeyboard);

                        return true;
                    }

                    var namePickerText = ClientData.RedisPersonData.IsUser
                        ? $"{Constants.UI.Icons.Common.MOON_1} {_localeManager.GetString("EnterYourPersonName", ClientData.AstroUser.CultureInfo)}"
                        : $"{Constants.UI.Icons.Common.MOON_1} {_localeManager.GetString("EnterPersonName", ClientData.AstroUser.CultureInfo)}";

                    var addPersonKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new []
                        {
                            _clientHelper.GetBackToPersonsButton(ClientData),
                            _clientHelper.GetCancelButtonWithEdit(ClientData)
                        }
                    });

                    await _clientHelper.EditMessage(
                        ClientData.AstroUserId,
                        ClientData.Message.Id,
                        $"{namePickerText}:",
                        addPersonKeyboard);

                    await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.PersonNamePicker);

                    _logger.LogInformation("Enter person name message is sent.");
                    return true;
                case Constants.UI.Buttons.Commands.EDIT_PERSON:
                    _logger.LogInformation("Edit persons button handling.");

                    _logger.LogInformation("Getting persons.");

                    await _clientHelper.SendPersons(ClientData, Constants.UI.Buttons.CommandTypes.EDIT);

                    await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.Persons);

                    _logger.LogInformation("Editing persons message is sent.");
                    return true;
                case Constants.UI.Buttons.Commands.DELETE_PERSON:
                    _logger.LogInformation("Delete persons button handling.");

                    _logger.LogInformation("Getting persons.");

                    await _clientHelper.SendPersons(ClientData, Constants.UI.Buttons.CommandTypes.DELETE);

                    await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.Persons);

                    _logger.LogInformation("Deleting persons message is sent.");
                    return true;
                case Constants.UI.Buttons.Commands.NATAL_CHART:
                    var natalPerson = ClientData.AstroUser.GetChosenPerson();

                    _logger.LogInformation("Natal chart button handling.");

                    if (natalPerson == null ||
                        natalPerson.BirthDate == null ||
                        natalPerson.TimeZoneOffset == null)
                    {
                        _logger.LogError("Empty person info error.");
                        return true;
                    }

                    _logger.LogInformation("Getting natal chart info.");

                    if (!natalPerson.Longitude.HasValue
                        || !natalPerson.Latitude.HasValue)
                    {
                        _logger.LogError("Empty location info error.");
                        return true;
                    }

                    var birthChart = await _calculationService.GetChartInfo(
                        natalPerson.BirthDate.Value,
                        natalPerson.TimeZoneOffset.Value,
                        natalPerson.Longitude.Value,
                        natalPerson.Latitude.Value,
                        ClientData.AstroUser.HouseSystem);

                    _logger.LogInformation("Getting natal planets message.");
                    var natalPlanetsMessage = _clientHelper.GetNatalPlanetsMessage(birthChart, natalPerson, ClientData.AstroUser.CultureInfo);

                    await _botClient.SendMessage(
                            chatId: ClientData.AstroUserId,
                            text: natalPlanetsMessage,
                            replyMarkup: null);

                    _logger.LogInformation("Getting natal houses message.");
                    var housesMessage = _clientHelper.GetHousesMessage(birthChart, natalPerson, ClientData.AstroUser.CultureInfo);

                    await _botClient.SendMessage(
                            chatId: ClientData.AstroUserId,
                            text: housesMessage,
                            replyMarkup: null);

                    _logger.LogInformation("Getting natal aspects info.");
                    var natalAspects = _calculationService.GetNatalAspects(birthChart);

                    _logger.LogInformation("Getting natal aspects message.");
                    var natalMessages = _clientHelper.GetChartMessages(natalAspects, natalPerson, ClientData.AstroUser.CultureInfo, ChartTypeEnum.Natal);

                    _logger.LogInformation("Sending chart info message.");
                    await _clientHelper.SendMessageHtml(ClientData.AstroUserId, natalMessages, cancelKeyboard);

                    _logger.LogInformation("Natal chart info message is sent.");
                    return true;
                case Constants.UI.Buttons.Commands.TRANSIT_DAILY_FORECAST:
                    var transitPerson = ClientData.AstroUser.GetChosenPerson();

                    _logger.LogInformation("Transit daily forecast button handling.");

                    if (transitPerson == null ||
                        transitPerson.BirthDate == null ||
                        transitPerson.TimeZoneOffset == null)
                    {
                        _logger.LogError("Empty user info error.");
                        return true;
                    }

                    await _botClient.SendMessage(
                            chatId: ClientData.AstroUserId,
                            text: $"{Constants.UI.Icons.Common.HOURGLASS} {_localeManager.GetString("ForecastProccessing", ClientData.AstroUser.CultureInfo)}",
                            replyMarkup: null);

                    var transitInterval = new TimeSpan(1, 0, 0, 0);

                    _logger.LogInformation("Getting transit aspects info.");

                    if (!transitPerson.Longitude.HasValue
                        || !transitPerson.Latitude.HasValue)
                    {
                        _logger.LogError("Empty location info error.");
                        return true;
                    }

                    var transitAspects = await _calculationService.GetTransitAspects(
                        transitPerson.BirthDate.Value,
                        transitPerson.TimeZoneOffset.Value,
                        utcNowDate,
                        transitInterval,
                        transitPerson.Longitude.Value,
                        transitPerson.Latitude.Value,
                        ClientData.AstroUser.HouseSystem);

                    _logger.LogInformation("Getting transit aspects message.");
                    var transitMessages = _clientHelper.GetChartMessages(transitAspects, transitPerson, ClientData.AstroUser.CultureInfo, ChartTypeEnum.Transit);

                    _logger.LogInformation("Sending transit daily forecast message.");
                    await _clientHelper.SendMessageHtml(ClientData.AstroUserId, transitMessages, cancelKeyboard);

                    _logger.LogInformation("Transit daily forecast message is sent.");
                    return true;
                case Constants.UI.Buttons.Commands.DIRECTION_FORECAST:
                    var directionPerson = ClientData.AstroUser.GetChosenPerson();

                    _logger.LogInformation("Direction forecast button handling.");

                    if (ClientData.AstroUser == null ||
                        ClientData.AstroUser?.BirthDate == null ||
                        ClientData.AstroUser?.TimeZoneOffset == null)
                    {
                        _logger.LogError("Empty user info error.");
                        return true;
                    }

                    _logger.LogInformation("Getting direction aspects info.");

                    if (!directionPerson.BirthDate.HasValue
                        || !directionPerson.TimeZoneOffset.HasValue
                        || !directionPerson.Longitude.HasValue
                        || !directionPerson.Latitude.HasValue)
                    {
                        _logger.LogError("Empty birthDate/timeZone/location info error.");
                        return true;
                    }

                    var directionAspects = await _calculationService.GetDirectionAspects(
                        directionPerson.BirthDate.Value,
                        directionPerson.TimeZoneOffset.Value,
                        utcNowDate,
                        directionPerson.Longitude.Value,
                        directionPerson.Latitude.Value,
                        ClientData.AstroUser.HouseSystem);

                    _logger.LogInformation("Getting direction aspects message.");
                    var directionMessages = _clientHelper.GetChartMessages(directionAspects, directionPerson, ClientData.AstroUser.CultureInfo, ChartTypeEnum.Direction);

                    _logger.LogInformation("Sending direction forecast message.");
                    await _clientHelper.SendMessageHtml(ClientData.AstroUserId, directionMessages, cancelKeyboard);

                    _logger.LogInformation("Direction forecast message is sent.");
                    return true;
                case Constants.UI.Buttons.Commands.POSITIVE_FORECAST:
                    //TODO
                    return true;
                case Constants.UI.Buttons.Commands.SET_BIRTH_LOCATION:
                    _logger.LogInformation("Set birth location button handling.");

                    await SendLocation(
                        ClientData,
                        $"\n\n{Constants.UI.Icons.Common.EARTH}{_localeManager.GetString("ChooseBirthLocation", ClientData.AstroUser.CultureInfo)}");

                    await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.BirthLocationPicker);
                    return true;
                case Constants.UI.Buttons.Commands.ADD_USER:
                    await _redisService.CreateUserData(ClientData.AstroUserId);

                    await _personDataPicker.EditToYearIntervalPicker(
                        ClientData,
                        $"{Constants.UI.Icons.Common.BLUE_SMALL_RHOMB}{_localeManager.GetString("ChooseBirthYear", ClientData.AstroUser.CultureInfo)}:");

                    await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.YearIntervalPicker);
                    break;

                case Constants.UI.Buttons.Commands.SAVE_BIRTHDAY:
                    var person = await _redisService.GetPersonData(ClientData.AstroUserId);

                    if (person.IsUser)
                    {
                        await _userProvider.EditUser(
                            ClientData.AstroUserId,
                            ClientData.RedisPersonData);

                        await _clientHelper.EditToMenu(ClientData);
                        await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.Menu);
                    }
                    else
                    {
                        await _userProvider.AddOrEditPerson(
                            ClientData.AstroUserId,
                            ClientData.RedisPersonData.EditingPersonId,
                            ClientData.RedisPersonData);

                        await _clientHelper.SendPersons(ClientData, Constants.UI.Buttons.CommandTypes.GET);
                        await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.Persons);
                    }

                    await _redisService.DeletePersonData(ClientData.AstroUserId);
                    return true;
                case Constants.UI.Buttons.Commands.CHANGE_BIRTHDAY:
                    await _personDataPicker.SendYearPicker(
                        ClientData,
                        $"{Constants.UI.Icons.Common.BLUE_SMALL_RHOMB}{_localeManager.GetString("ChooseBirthYear", ClientData.AstroUser.CultureInfo)}:");

                    await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.YearPicker);
                    return true;
                default:
                    return false;
            }

            return false;
        }

        private async Task ChangePerson(List<IAstroPerson> persons, long id, bool isUserChosen)
        {
            var choosenPerson = persons.FirstOrDefault(p =>
                p.IsUser == isUserChosen &&
                p.Id.HasValue &&
                p.Id.Value == id);

            if (choosenPerson == null)
            {
                var personType = isUserChosen ? "user" : "person";
                _logger.LogError($"Can't find {personType} with id = \"{id}\".");
                return;
            }

            if (choosenPerson.IsChosen == true)
            {
                return;
            }

            var currentChosenPerson = persons.FirstOrDefault(p =>
                p.IsChosen == true);

            if (currentChosenPerson == null)
            {
                _logger.LogError($"Can't find chosen person (or user). User id = \"{id}\".");
                return;
            }

            choosenPerson.IsChosen = true;
            currentChosenPerson.IsChosen = false;

            await _userProvider.UpdateChoosePersons(currentChosenPerson, choosenPerson);
        }


        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            //TODO
            throw new NotImplementedException();
        }

        private async Task<AstroUser> GetOrCreateUser(Update update)
        {
            _logger.LogInformation($"Receiving or creating user.");

            var chatId = update?.CallbackQuery?.Message?.Chat?.Id
                ?? update?.Message?.Chat?.Id;

            if (!chatId.HasValue)
            {
                _logger.LogError("Getting user chat id error.");
                throw new NullReferenceException("Getting user chat id error");
            }

            var astroUser = await _userProvider.GetUser(chatId.Value);

            if (astroUser == null)
            {
                _logger.LogInformation($"Creating new user.");

                var defaultLanguage = update?.CallbackQuery?.From?.LanguageCode
                    ?? update?.Message?.From?.LanguageCode
                    ?? "en";

                astroUser = new AstroUser()
                {
                    Id = chatId,
                    BirthDate = null,
                    TimeZoneOffset = null,
                    Language = defaultLanguage,
                    HouseSystem = HouseSystemEnum.Placidus
                };

                await _userProvider.AddUser(astroUser);
            }
            else
            {
                _logger.LogInformation($"User info is received.");
            }

            return astroUser;
        }

        private async Task SetLanguage()
        {
            _logger.LogInformation("Sending language picker.");

            await _clientHelper.SendLanguagePicker(ClientData);
            await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.LanguagePicker);
        }

        private async Task SetHouseSystem()
        {
            _logger.LogInformation("Sending house system picker.");

            await _clientHelper.SendHouseSystemPicker(ClientData);
            await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.HouseSystemPicker);
        }

        private async Task SetBirthday()
        {
            _logger.LogInformation("Sending year interval picker.");

            var isNewUser = await _userProvider.IsNewUser(ClientData.AstroUserId);

            if (isNewUser)
            {
                await _redisService.CreateUserData(ClientData.AstroUserId);

                await _personDataPicker.SendYearIntervalPicker(
                    ClientData,
                    $"{Constants.UI.Icons.Common.BLUE_SMALL_RHOMB}{_localeManager.GetString("ChooseBirthYear", ClientData.AstroUser.CultureInfo)}:");
            }
            else
            {
                await BeginEditingUser(true);
            }

            await _userProvider.SetUserStage(ClientData.AstroUserId, ChatStageEnum.YearIntervalPicker);
        }

        private async Task SendLocation(TBotClientData clientData, string text)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
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
                replyMarkup: inlineKeyboard);
        }
    }
}
