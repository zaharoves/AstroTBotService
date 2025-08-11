using AstroTBotService.AstroCalculation.Entities;
using AstroTBotService.Db.Entities;
using AstroTBotService.Entities;
using AstroTBotService.Enums;
using System.Globalization;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace AstroTBotService.TBot
{
    public class TClientHelper : ITClientHelper
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IResourcesLocaleManager _localeManager;
        private readonly ILogger<TClientHelper> _logger;

        public TClientHelper(
            ITelegramBotClient botClient,
            IResourcesLocaleManager localeManager,
            ILogger<TClientHelper> logger)
        {
            _botClient = botClient;
            _localeManager = localeManager;
            _logger = logger;
        }

        public async Task SendMessage(long chatId, string message, ReplyMarkup replyMarkup)
        {
            try
            {
                if (message.Length > 4096)
                {
                    message = message.Substring(0, 4096);
                }

                await _botClient.SendMessage(
                    chatId: chatId,
                    text: message,
                    replyMarkup: replyMarkup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sending message error. Chat Id: {chatId}.");
            }
        }

        public async Task EditMessage(long chatId, int messageId, string messageText, InlineKeyboardMarkup replyMarkup)
        {
            try
            {
                if (messageText.Length > 4096)
                {
                    messageText = messageText.Substring(0, 4096);
                }

                await _botClient.EditMessageText(
                    chatId: chatId,
                    messageId: messageId,
                    text: messageText,
                    replyMarkup: replyMarkup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Editing message error. Chat Id: {chatId}.");
            }
        }

        public async Task SendMessageHtml(long chatId, List<string> messages, ReplyMarkup replyMarkup)
        {
            try
            {
                var sendMessage = string.Empty;

                for (var i = 0; i < messages.Count(); i++)
                {
                    if (messages[i].Length > Constants.UI.MAX_T_MESSAGE_LENGTH)
                    {
                        _logger.LogError($"Too long message for sending. Length {messages[i].Length}. Chat Id: {chatId}.");
                        continue;
                    }

                    if (sendMessage.Length + messages[i].Length <= Constants.UI.MAX_T_MESSAGE_LENGTH)
                    {
                        sendMessage += messages[i];
                        continue;
                    }

                    await _botClient.SendMessage(
                    chatId: chatId,
                    text: sendMessage,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);

                    sendMessage = messages[i];
                }

                if (!string.IsNullOrEmpty(sendMessage))
                {
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: sendMessage,
                        replyMarkup: replyMarkup,
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sending HTML message error. Chat Id: {chatId}.");
            }
        }

        public async Task SendMenu(TBotClientData clientData)
        {
            var menuInfo = GetMenuKeyboard(clientData);

            await _botClient.SendMessage(
                chatId: clientData.AstroUserId,
                text: menuInfo.Message,
                replyMarkup: menuInfo.Keyboard);
        }

        public async Task EditToMenu(TBotClientData clientData)
        {
            var menuInfo = GetMenuKeyboard(clientData);

            await _botClient.EditMessageText(
                chatId: clientData.AstroUserId,
                messageId: clientData.Message.Id,
                text: menuInfo.Message,
                replyMarkup: menuInfo.Keyboard);
        }

        public async Task SendLanguagePicker(TBotClientData clientData)
        {
            var menuInfo = GetChooseLanguageKeyboard(clientData);

            await _botClient.SendMessage(
                chatId: clientData.AstroUserId,
                text: menuInfo.Message,
                replyMarkup: menuInfo.Keyboard);
        }

        public async Task SendHouseSystemPicker(TBotClientData clientData)
        {
            var menuInfo = GetHousePickerKeyboard(clientData);

            await _botClient.SendMessage(
                chatId: clientData.AstroUserId,
                text: menuInfo.Message,
                replyMarkup: menuInfo.Keyboard);
        }

        public async Task SendPersons(TBotClientData clientData, string commandButtonPrefix)
        {
            var menuInfo = GetPersonsKeyboard(clientData, commandButtonPrefix);

            await _botClient.EditMessageText(
                chatId: clientData.AstroUserId,
                messageId: clientData.Message.Id,
                text: menuInfo.Message,
                replyMarkup: menuInfo.Keyboard);
        }

        public async Task SendEditPersons(TBotClientData clientData)
        {
            var message = _localeManager.GetString("ChooseEditPerson", clientData.AstroUser.CultureInfo);
            var buttons = GetCancelButtonWithEdit(clientData);

            await _botClient.EditMessageText(
                chatId: clientData.AstroUserId,
                messageId: clientData.Message.Id,
                text: $"{message}:",
                replyMarkup: buttons);
        }

        public InlineKeyboardButton GetBackToPersonsButton(TBotClientData clientData)
        {
            var buttonText = _localeManager.GetString("Back", clientData.AstroUser.CultureInfo);

            return InlineKeyboardButton.WithCallbackData($"{buttonText} {Constants.UI.Icons.Common.BACK}", $"{Constants.UI.Buttons.Commands.GET_PERSONS}");
        }

        public InlineKeyboardButton GetCancelButton(TBotClientData clientData)
        {
            var buttonText = _localeManager.GetString("Cancel", clientData.AstroUser.CultureInfo);

            return InlineKeyboardButton.WithCallbackData($"{buttonText} {Constants.UI.Icons.Common.ORANGE_CIRCLE}", $"{Constants.UI.Buttons.Commands.SEND_MENU}");
        }

        public InlineKeyboardButton GetCancelButtonWithEdit(TBotClientData clientData)
        {
            var buttonText = _localeManager.GetString("Cancel", clientData.AstroUser.CultureInfo);

            return InlineKeyboardButton.WithCallbackData($"{buttonText} {Constants.UI.Icons.Common.ORANGE_CIRCLE}", $"{Constants.UI.Buttons.Commands.EDIT_TO_MENU}");
        }

        public InlineKeyboardButton GetCancelButton(TBotClientData clientData, string buttonText)
        {
            return InlineKeyboardButton.WithCallbackData($"{buttonText} {Constants.UI.Icons.Common.ORANGE_CIRCLE}", $"{Constants.UI.Buttons.Commands.SEND_MENU}");
        }

        public InlineKeyboardButton GetCancelButtonWithEdit(TBotClientData clientData, string buttonText)
        {
            return InlineKeyboardButton.WithCallbackData($"{buttonText} {Constants.UI.Icons.Common.ORANGE_CIRCLE}", $"{Constants.UI.Buttons.Commands.EDIT_TO_MENU}");
        }

        private (string Message, InlineKeyboardMarkup Keyboard) GetMenuKeyboard(TBotClientData clientData)
        {
            var message = string.Empty;
            var keyboard = new InlineKeyboardMarkup();
            var cultureInfo = clientData.AstroUser.CultureInfo;

            Constants.UI.FlagsInfoDict.TryGetValue(clientData.AstroUser.Language, out (string Icon, string Description) flagData);

            var person = clientData.AstroUser.GetChosenPerson();

            if (person.BirthDate == null)
            {
                message = $"{Constants.UI.Icons.Common.SCIENCE} {_localeManager.GetString("FillYourBirthDate", cultureInfo)}.";

                var setBirthDateButton = new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        _localeManager.GetString("SetBirthDate", cultureInfo),
                        Constants.UI.Buttons.Commands.ADD_USER)
                };

                keyboard = GetKeyboard(setBirthDateButton);
            }
            else
            {
                var birthdayMessage = person.IsUser == true
                    ? $"{_localeManager.GetString("YourBirthDate", cultureInfo)}:"
                    : $"{person.Name}\n{_localeManager.GetString("BirthDate", cultureInfo)}:";

                message = $"{Constants.UI.Icons.Common.SUN} {birthdayMessage}\n" +
                    $"{person.DateToLongString(cultureInfo)}";

                if (person.Longitude != null && person.Latitude != null)
                {
                    message += $"\n\n{Constants.UI.Icons.Common.EARTH} {_localeManager.GetString("Location", cultureInfo)}:\n" +
                        $"{_localeManager.GetString("Longitude", cultureInfo)}: {person.Longitude.Value.ToString("F6")}\n" +
                        $"{_localeManager.GetString("Latitude", cultureInfo)}: {person.Latitude.Value.ToString("F6")}";

                    message += $"\n\n{Constants.UI.Icons.Common.HOUSE} {_localeManager.GetString("HousesSystem", cultureInfo)}:\n" +
                        $"{_localeManager.GetString(clientData.AstroUser.HouseSystem.ToString(), cultureInfo)}";

                }

                message += $"\n\n{_localeManager.GetString("YouCanCalculate", cultureInfo)}.";

                message += $"\n{_localeManager.GetString("YouCanChangeConfig", cultureInfo)}.";

                var personsButton = new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"{Constants.UI.Icons.Common.FACE_MAN_1} {Constants.UI.Icons.Common.FACE_WOMAN_1} " +
                        $"{_localeManager.GetString("Database", cultureInfo)}",
                        Constants.UI.Buttons.Commands.GET_PERSONS)
                };

                var natalIcon = string.Empty;
                if (Constants.UI.ChartTypeIconDict.TryGetValue(ChartTypeEnum.Natal, out var _natalIcon))
                {
                    natalIcon = _natalIcon;
                }

                var natalCharButton = new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                    $"{natalIcon} {_localeManager.GetString("NatalChartButton", cultureInfo)}",
                    Constants.UI.Buttons.Commands.NATAL_CHART)
                };

                var transitIcon = string.Empty;
                if (Constants.UI.ChartTypeIconDict.TryGetValue(ChartTypeEnum.Transit, out var _transitIcon))
                {
                    transitIcon = _transitIcon;
                }

                var transitForecastButton = new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                    $"{transitIcon} {_localeManager.GetString("TransitForecastButton", cultureInfo)}",
                    Constants.UI.Buttons.Commands.TRANSIT_DAILY_FORECAST),
                };

                var directionIcon = string.Empty;
                if (Constants.UI.ChartTypeIconDict.TryGetValue(ChartTypeEnum.Direction, out var _directionIcon))
                {
                    directionIcon = _directionIcon;
                }

                var directionForecastButton = new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                    $"{directionIcon} {_localeManager.GetString("DirectionForecastButton", cultureInfo)}",
                    Constants.UI.Buttons.Commands.DIRECTION_FORECAST),
                };

                keyboard = GetKeyboard(
                    personsButton,
                    natalCharButton,
                    transitForecastButton,
                    directionForecastButton);
            }

            return (message, keyboard);
        }

        private InlineKeyboardMarkup GetKeyboard(params InlineKeyboardButton[][] buttons)
        {
            return new InlineKeyboardMarkup(buttons);
        }

        private (string Message, InlineKeyboardMarkup Keyboard) GetChooseLanguageKeyboard(TBotClientData clientData)
        {
            var message = $"{_localeManager.GetString("ChooseLanguage", clientData.AstroUser.CultureInfo)}:";

            var buttons = new List<InlineKeyboardButton[]>();

            foreach (var flagInfo in Constants.UI.FlagsInfoDict)
            {
                buttons.Add(
                [
                    InlineKeyboardButton.WithCallbackData(
                        $"{flagInfo.Value.Icon} {flagInfo.Value.Description}",
                        flagInfo.Key)
                ]);
            }

            buttons.Add(
            [
                GetCancelButtonWithEdit(clientData)
            ]);

            return (message, new InlineKeyboardMarkup(buttons));
        }

        private (string Message, InlineKeyboardMarkup Keyboard) GetPersonsKeyboard(TBotClientData clientData, string commandButtonPrefix)
        {
            var icon = clientData.AstroUser.IsChosen == true && commandButtonPrefix == Constants.UI.Buttons.CommandTypes.GET
                ? $"{Constants.UI.Icons.Common.CHOOSED} "
                : string.Empty;

            var buttons = new List<InlineKeyboardButton[]>();

            if (commandButtonPrefix != Constants.UI.Buttons.CommandTypes.DELETE)
            {
                buttons.Add(
                [
                    InlineKeyboardButton.WithCallbackData(
                    $"{icon}{_localeManager.GetString("You", clientData.AstroUser.CultureInfo)}" +
                    $"{Constants.UI.Icons.Common.MINUS}" +
                    $"{clientData.AstroUser.DateToShortString(clientData.AstroUser.CultureInfo)}",
                    $"{Constants.UI.Buttons.PersonTypes.USER}" +
                    $"{Constants.UI.Buttons.SEPARATOR}" +
                    $"{commandButtonPrefix}" +
                    $"{Constants.UI.Buttons.SEPARATOR}" +
                    $"{clientData.AstroUserId}")
                ]);
            }

            if ((clientData?.AstroUser?.ChildPersons?.Count() ?? 0) != 0)
            {
                foreach (var person in clientData.AstroUser.ChildPersons.OrderBy(p => p.Name))
                {
                    icon = person.IsChosen == true && commandButtonPrefix == Constants.UI.Buttons.CommandTypes.GET
                        ? $"{Constants.UI.Icons.Common.CHOOSED} "
                        : string.Empty;

                    buttons.Add(
                    [
                        InlineKeyboardButton.WithCallbackData(
                        $"{icon}{person?.Name}" +
                        $"{Constants.UI.Icons.Common.MINUS}" +
                        $"{person.DateToShortString(clientData.AstroUser.CultureInfo)}",
                        $"{Constants.UI.Buttons.PersonTypes.PERSON}" +
                        $"{Constants.UI.Buttons.SEPARATOR}" +
                        $"{commandButtonPrefix}" +
                        $"{Constants.UI.Buttons.SEPARATOR}" +
                        $"{person.Id}")
                    ]);
                }
            }

            var message = string.Empty;

            switch (commandButtonPrefix)
            {
                case Constants.UI.Buttons.CommandTypes.GET:
                    message = $"{_localeManager.GetString("ChoosePersonMessage", clientData.AstroUser.CultureInfo)}:";
                    buttons.Add(new[] { GetAddPersonButton(clientData), GetEditPersonButton(clientData) });
                    buttons.Add(new[] { GetDeletePersonButton(clientData), GetCancelButtonWithEdit(clientData) });
                    break;
                case Constants.UI.Buttons.CommandTypes.EDIT:

                    message = $"{Constants.UI.Icons.Common.EDIT} {_localeManager.GetString("ChooseEditPersonMessage", clientData.AstroUser.CultureInfo)}:";
                    buttons.Add(new[] { GetBackToPersonsButton(clientData), GetCancelButtonWithEdit(clientData) });
                    break;
                case Constants.UI.Buttons.CommandTypes.DELETE:
                    message = $"{Constants.UI.Icons.Common.X_RED} {_localeManager.GetString("ChooseDeletePersonMessage", clientData.AstroUser.CultureInfo)}:";
                    buttons.Add(new[] { GetBackToPersonsButton(clientData), GetCancelButtonWithEdit(clientData) });
                    break;
            }

            return (message, new InlineKeyboardMarkup(buttons));
        }

        private (string Message, InlineKeyboardMarkup Keyboard) GetHousePickerKeyboard(TBotClientData clientData)
        {
            var message = $"{_localeManager.GetString("ChooseHousesSystem", clientData.AstroUser.CultureInfo)}:";

            var buttons = new List<InlineKeyboardButton[]>();

            foreach (var houseSystem in Enum.GetValues(typeof(HouseSystemEnum)).Cast<HouseSystemEnum>().Order())
            {
                if (houseSystem == clientData.AstroUser.HouseSystem)
                {
                    buttons.Add(
                    [
                        InlineKeyboardButton.WithCallbackData(
                            $"{Constants.UI.Icons.Common.CHOOSED} {_localeManager.GetString(houseSystem.ToString(), clientData.AstroUser.CultureInfo)}",
                            ((int)houseSystem).ToString())
                    ]);

                    continue;
                }

                buttons.Add(
                [
                    InlineKeyboardButton.WithCallbackData(
                        $"{_localeManager.GetString(houseSystem.ToString(), clientData.AstroUser.CultureInfo)}",
                        ((int)houseSystem).ToString())
                ]);
            }

            buttons.Add(
            [
                GetCancelButtonWithEdit(clientData)
            ]);

            return (message, new InlineKeyboardMarkup(buttons));
        }

        public InlineKeyboardButton GetAddPersonButton(TBotClientData clientData)
        {
            var buttonText = _localeManager.GetString("Add", clientData.AstroUser.CultureInfo);

            return InlineKeyboardButton.WithCallbackData($"{buttonText} {Constants.UI.Icons.Common.GREEN_CIRCLE}", $"{Constants.UI.Buttons.Commands.ADD_PERSON}");
        }

        public InlineKeyboardButton GetEditPersonButton(TBotClientData clientData)
        {
            var buttonText = _localeManager.GetString("Change", clientData.AstroUser.CultureInfo);

            return InlineKeyboardButton.WithCallbackData($"{buttonText} {Constants.UI.Icons.Common.YELLOW_CIRCLE}", $"{Constants.UI.Buttons.Commands.EDIT_PERSON}");
        }

        public InlineKeyboardButton GetDeletePersonButton(TBotClientData clientData)
        {
            var buttonText = _localeManager.GetString("Delete", clientData.AstroUser.CultureInfo);

            return InlineKeyboardButton.WithCallbackData($"{buttonText} {Constants.UI.Icons.Common.RED_CIRCLE}", $"{Constants.UI.Buttons.Commands.DELETE_PERSON}");
        }

        public List<string> GetChartMessages(
            List<AspectInfo> aspects,
            IAstroPerson astroPerson,
            CultureInfo cultureInfo,
            ChartTypeEnum chartTypeEnum)
        {
            var messages = new List<string>();

            var icon = string.Empty;

            if (Constants.UI.ChartTypeIconDict.TryGetValue(chartTypeEnum, out var _icon))
            {
                icon = _icon;
            }

            var chartType = chartTypeEnum.ToString() + "AspectsInfo";

            var name = astroPerson.IsUser == true
                ? _localeManager.GetString("You", cultureInfo)
                : astroPerson.Name;

            messages.Add($"{icon} {name}\n" +
                $"{astroPerson.DateToLongString(cultureInfo)}\n" +
                $"{_localeManager.GetString(chartType, cultureInfo)}:\n\n");

            foreach (var aspect in aspects)
            {
                var sb = new StringBuilder();

                string transitAnglesStr = (aspect.TransitPlanet.ZodiacAngles.ToString().Length < 2)
                    ? $"0{aspect.TransitPlanet.ZodiacAngles}"
                    : aspect.TransitPlanet.ZodiacAngles.ToString();

                string transitMinutesStr = (aspect.TransitPlanet.ZodiacMinutes.ToString().Length < 2)
                    ? $"0{aspect.TransitPlanet.ZodiacMinutes}"
                    : aspect.TransitPlanet.ZodiacMinutes.ToString();

                string natalAnglesStr = (aspect.NatalPlanet.ZodiacAngles.ToString().Length < 2)
                    ? $"0{aspect.NatalPlanet.ZodiacAngles}"
                    : aspect.NatalPlanet.ZodiacAngles.ToString();

                string natalMinutesStr = (aspect.NatalPlanet.ZodiacMinutes.ToString().Length < 2)
                    ? $"0{aspect.NatalPlanet.ZodiacMinutes}"
                    : aspect.NatalPlanet.ZodiacMinutes.ToString();

                var transitPlanetIcon = Constants.UI.PlanetIconDict[aspect.TransitPlanet.Planet];
                var transitZodiacIcon = Constants.UI.ZodiacIconDict[aspect.TransitPlanet.Zodiac];

                var natalPlanetIcon = Constants.UI.PlanetIconDict[aspect.NatalPlanet.Planet];
                var natalZodiacIcon = Constants.UI.ZodiacIconDict[aspect.NatalPlanet.Zodiac];

                var aspectIcon = Constants.UI.AspectIconDict[aspect.Aspect];

                var transitRetroIcon = aspect.TransitPlanet.IsRetro ? Constants.UI.Icons.Common.RETRO : string.Empty;
                var natalRetroIcon = aspect.NatalPlanet.IsRetro ? Constants.UI.Icons.Common.RETRO : string.Empty;

                var transitRetroStr = aspect.TransitPlanet.IsRetro ? "(R)" : string.Empty;
                var natalRetroStr = aspect.NatalPlanet.IsRetro ? "(R)" : string.Empty;

                var transitStr = $"{transitPlanetIcon}{transitRetroIcon}  {transitZodiacIcon}" +
                    $"[{transitAnglesStr}{Constants.UI.Icons.Common.ANGLES}{transitMinutesStr}{Constants.UI.Icons.Common.MINUTES}]";

                if (chartTypeEnum == ChartTypeEnum.Natal)
                {
                    transitStr += $" ( {aspect.TransitPlanet.StandHouse} )";
                }

                var natalStr = $"{natalPlanetIcon}{natalRetroIcon}  {natalZodiacIcon}" +
                    $"[{natalAnglesStr}{Constants.UI.Icons.Common.ANGLES}{natalMinutesStr}{Constants.UI.Icons.Common.MINUTES}]";

                if (chartTypeEnum == ChartTypeEnum.Natal)
                {
                    natalStr += $" ( {aspect.NatalPlanet.StandHouse} )";
                }

                sb.Append($"{_localeManager.GetString(aspect.TransitPlanet.Planet.ToString(), cultureInfo)}" +
                    $"{transitRetroStr} " +
                    $"{Constants.UI.Icons.Common.MINUS} " +
                    $"{_localeManager.GetString(aspect.Aspect.ToString(), cultureInfo)} " +
                    $"{Constants.UI.Icons.Common.MINUS} " +
                    $"{_localeManager.GetString(aspect.NatalPlanet.Planet.ToString(), cultureInfo)}" +
                    $"{natalRetroStr}");

                if (chartTypeEnum == ChartTypeEnum.Transit &&
                    aspect.TransitPlanet.Planet == PlanetEnum.Moon)
                {
                    sb.Append($" [{aspect.StartDate.ToString("HH:mm")} - {aspect.EndDate.ToString("HH:mm")}]");
                }

                sb.Append($"\n{transitStr}   {aspectIcon}   {natalStr}");

                sb.Append("\n");

                var description = GetAspectDescription(aspect, cultureInfo, chartTypeEnum);

                var expandableDescription = $@"<blockquote expandable>{description}</blockquote> ";

                sb.Append(expandableDescription);
                sb.Append("\n");

                messages.Add(sb.ToString());
            }

            return messages;
        }

        public string GetNatalPlanetsMessage(ChartInfo chartInfo, IAstroPerson astroPerson, CultureInfo cultureInfo)
        {
            var sBuilder = new StringBuilder();

            var name = astroPerson.IsUser == true
                ? _localeManager.GetString("You", cultureInfo)
                : astroPerson.Name;

            sBuilder.Append($"{Constants.UI.Icons.Common.WHITE_CIRCLE} {name}\n" +
                $"{astroPerson.DateToLongString(cultureInfo)}\n" +
                $"{_localeManager.GetString("NatalPlanetsInfo", cultureInfo)}:\n\n");

            var i = 0;

            foreach (var planet in chartInfo.Planets.Values)
            {
                string anglesStr = (planet.ZodiacAngles.ToString().Length < 2)
                    ? $"0{planet.ZodiacAngles}"
                    : planet.ZodiacAngles.ToString();

                string minutesStr = (planet.ZodiacMinutes.ToString().Length < 2)
                    ? $"0{planet.ZodiacMinutes}"
                    : planet.ZodiacMinutes.ToString();

                var planetIcon = Constants.UI.PlanetIconDict[planet.Planet];
                var zodiacIcon = Constants.UI.ZodiacIconDict[planet.Zodiac];

                var retroIcon = planet.IsRetro ? Constants.UI.Icons.Common.RETRO : string.Empty;
                var retroStr = planet.IsRetro ? " (R)" : string.Empty;

                var planetStr = $"{planetIcon}{retroIcon}  {zodiacIcon}" +
                    $"[{anglesStr}{Constants.UI.Icons.Common.ANGLES}{minutesStr}{Constants.UI.Icons.Common.MINUTES}]";

                var rulesHouses = string.Join(", ", planet.RulerHouses);

                sBuilder.Append($"{_localeManager.GetString(planet.Planet.ToString(), cultureInfo)}" +
                    $"{retroStr} " +
                    $"{Constants.UI.Icons.Common.MINUS} " +
                    $"{planetStr} " +
                    $"{Constants.UI.Icons.Common.MINUS} " +
                    $"( {planet.StandHouse} )" +
                    $"{Constants.UI.Icons.Common.MINUS} " +
                    $"[{rulesHouses}]\n");

                i++;

                if (i == 5)
                {
                    sBuilder.Append("\n");
                }
            }

            return sBuilder.ToString();
        }

        public string GetHousesMessage(ChartInfo chartInfo, IAstroPerson astroPerson, CultureInfo cultureInfo)
        {
            var sBuilder = new StringBuilder();

            var name = astroPerson.IsUser == true
                ? _localeManager.GetString("You", cultureInfo)
                : astroPerson.Name;


            sBuilder.Append($"{Constants.UI.Icons.Common.WHITE_CIRCLE} {name}\n" +
                $"{astroPerson.DateToLongString(cultureInfo)}\n" +
                $"{_localeManager.GetString("NatalHousesInfo", cultureInfo)}:\n\n");

            var i = 0;

            foreach (var house in chartInfo.Houses)
            {
                string anglesStr = (house.Value.ZodiacAngles.ToString().Length < 2)
                    ? $"0{house.Value.ZodiacAngles}"
                    : house.Value.ZodiacAngles.ToString();

                string minutesStr = (house.Value.ZodiacMinutes.ToString().Length < 2)
                    ? $"0{house.Value.ZodiacMinutes}"
                    : house.Value.ZodiacMinutes.ToString();

                string secondsStr = (house.Value.ZodiacSeconds.ToString().Length < 2)
                    ? $"0{house.Value.ZodiacSeconds}"
                    : house.Value.ZodiacSeconds.ToString();

                var zodiacIcon = Constants.UI.ZodiacIconDict[house.Value.Zodiac];

                var houseStr = $"{zodiacIcon} " +
                    $"[{anglesStr}{Constants.UI.Icons.Common.ANGLES}" +
                    $"{minutesStr}{Constants.UI.Icons.Common.MINUTES}" +
                    $"{secondsStr}{Constants.UI.Icons.Common.SECONDS}]";

                sBuilder.Append($"{house.Key}" +
                    $"{Constants.UI.Icons.Common.MINUS} " +
                    $"{houseStr}\n");

                i++;

                if (i == 3)
                {
                    sBuilder.Append("\n");
                    i = 0;
                }
            }

            return sBuilder.ToString();
        }

        private string GetAspectDescription(AspectInfo aspect, CultureInfo cultureInfo, ChartTypeEnum chartTypeEnum)
        {
            var aspectName =
                chartTypeEnum.ToString() +
                aspect.Aspect.ToString() +
                aspect.NatalPlanet.Planet.ToString();

            if (_localeManager.TryGetString(aspectName, cultureInfo, out var firstDescription))
            {
                return firstDescription;
            }

            aspectName =
                chartTypeEnum.ToString() +
                aspect.NatalPlanet.Planet.ToString() +
                aspect.Aspect.ToString() +
                aspect.TransitPlanet.Planet.ToString();

            if (_localeManager.TryGetString(aspectName, cultureInfo, out var secondDescription))
            {
                return secondDescription;
            }

            return string.Empty;
        }
    }
}
