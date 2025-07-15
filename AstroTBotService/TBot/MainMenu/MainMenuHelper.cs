using AstroTBotService.AstroCalculation.Entities;
using AstroTBotService.Entities;
using AstroTBotService.Enums;
using System.Globalization;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using static AstroTBotService.Constants.UI;

namespace AstroTBotService.TBot
{
    public class MainMenuHelper : IMainMenuHelper
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IResourcesLocaleManager _localeManager;
        private readonly ILogger<MainMenuHelper> _logger;

        public MainMenuHelper(
            ITelegramBotClient botClient,
            IResourcesLocaleManager localeManager,
            ILogger<MainMenuHelper> logger)
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
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                Console.WriteLine($"Ошибка Telegram API при отправке сообщения в чат {chatId}: {ex.Message}");
                Console.WriteLine($"Код ошибки: {ex.ErrorCode}");
                Console.WriteLine($"Описание ошибки: {ex.Parameters}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла непредвиденная ошибка при отправке сообщения в чат {chatId}: {ex.Message}");
                Console.WriteLine($"Стек вызова: {ex.StackTrace}");
            }
        }

        public async Task SendMessageHtml(long chatId, List<string> messages, ReplyMarkup replyMarkup)
        {
            try
            {
                var sendMessage = string.Empty;

                for (var i = 0; i < messages.Count(); i++)
                {
                    if (messages[i].Length > Constants.MAX_T_MESSAGE_LENGTH)
                    {
                        Console.WriteLine($"Слишком длинное сообщение. Не будет отправлено!");
                        continue;
                    }

                    if (sendMessage.Length + messages[i].Length <= Constants.MAX_T_MESSAGE_LENGTH)
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
                    //TODO !!!
                    //var keyboard = new InlineKeyboardMarkup(new[]
                    //{
                    //    //new [] { GetCancelButton() }
                    //});

                    await _botClient.SendMessage(
                    chatId: chatId,
                    text: sendMessage,
                    replyMarkup: replyMarkup,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                }
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                Console.WriteLine($"Ошибка Telegram API при отправке сообщения в чат {chatId}: {ex.Message}");
                Console.WriteLine($"Код ошибки: {ex.ErrorCode}");
                Console.WriteLine($"Описание ошибки: {ex.Parameters}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла непредвиденная ошибка при отправке сообщения в чат {chatId}: {ex.Message}");
                Console.WriteLine($"Стек вызова: {ex.StackTrace}");
            }
        }

        public async Task SendMainMenu(TBotClientData clientData)
        {
            var menuInfo = GetMainMenuKeyboard(clientData);

            await _botClient.SendMessage(
                chatId: clientData.AstroUser.Id,
                text: menuInfo.Message,
                replyMarkup: menuInfo.Keyboard);
        }

        public async Task EditToMainMenu(TBotClientData clientData)
        {
            var menuInfo = GetMainMenuKeyboard(clientData);

            await _botClient.EditMessageText(
                chatId: clientData.AstroUser.Id,
                messageId: clientData.Message.Id,
                text: menuInfo.Message,
                replyMarkup: menuInfo.Keyboard);
        }

        public async Task SendLanguagePicker(TBotClientData clientData)
        {
            var menuInfo = GetChooseLanguageKeyboard(clientData);

            await _botClient.SendMessage(
                chatId: clientData.AstroUser.Id,
                text: menuInfo.Message,
                replyMarkup: menuInfo.Keyboard);
        }

        public async Task EditToLanguagePicker(TBotClientData clientData)
        {
            var menuInfo = GetChooseLanguageKeyboard(clientData);

            await _botClient.EditMessageText(
                chatId: clientData.AstroUser.Id,
                messageId: clientData.Message.Id,
                text: menuInfo.Message,
                replyMarkup: menuInfo.Keyboard);
        }



        public InlineKeyboardButton GetCancelButton(TBotClientData clientData)
        {
            var buttonText = _localeManager.GetString("Cancel", clientData.CultureInfo);

            return InlineKeyboardButton.WithCallbackData($"{buttonText} {Constants.UI.Icons.Common.ORANGE_CIRCLE}", $"{Constants.UI.ButtonCommands.SEND_MAIN_MENU}");
        }

        public InlineKeyboardButton GetCancelButtonWithEdit(TBotClientData clientData)
        {
            var buttonText = _localeManager.GetString("Cancel", clientData.CultureInfo);

            return InlineKeyboardButton.WithCallbackData($"{buttonText} {Constants.UI.Icons.Common.ORANGE_CIRCLE}", $"{Constants.UI.ButtonCommands.EDIT_TO_MAIN_MENU}");
        }

        public InlineKeyboardButton GetCancelButton(TBotClientData clientData, string buttonText)
        {
            return InlineKeyboardButton.WithCallbackData($"{buttonText} {Constants.UI.Icons.Common.ORANGE_CIRCLE}", $"{Constants.UI.ButtonCommands.SEND_MAIN_MENU}");
        }

        public InlineKeyboardButton GetCancelButtonWithEdit(TBotClientData clientData, string buttonText)
        {
            return InlineKeyboardButton.WithCallbackData($"{buttonText} {Constants.UI.Icons.Common.ORANGE_CIRCLE}", $"{Constants.UI.ButtonCommands.EDIT_TO_MAIN_MENU}");
        }

        private (string Message, InlineKeyboardMarkup Keyboard) GetMainMenuKeyboard(TBotClientData clientData)
        {
            var message = string.Empty;
            var keyboard = new InlineKeyboardMarkup();

            Constants.UI.FlagsInfoDict.TryGetValue(clientData.AstroUser.Language, out (string Icon, string Description) flagData);

            if (clientData.AstroUser.BirthDate != null)
            {
                message = $"{Constants.UI.Icons.Common.SUN} {_localeManager.GetString("YourBirthDate", clientData.CultureInfo)}\n" +
                    $"{clientData.AstroUser.DateToString(clientData.CultureInfo)}";

                if (clientData.AstroUser.Longitude != null && clientData.AstroUser.Latitude != null)
                {
                    message += $"\n\n{Constants.UI.Icons.Common.EARTH} {_localeManager.GetString("YourLocation", clientData.CultureInfo)}\n" +
                        $"{_localeManager.GetString("Longitude", clientData.CultureInfo)}: {clientData.AstroUser.Longitude.Value.ToString("F6")}\n" +
                        $"{_localeManager.GetString("Latitude", clientData.CultureInfo)}: {clientData.AstroUser.Latitude.Value.ToString("F6")}";
                }

                message += $"\n\n{_localeManager.GetString("YouCanCalculate", clientData.CultureInfo)}.";

                message += $"\n{_localeManager.GetString("YouCanChangeConfig", clientData.CultureInfo)}.";

                var setLocationButton = new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                    $"{Constants.UI.Icons.Common.EARTH} {_localeManager.GetString("SetBirthLocation", clientData.CultureInfo)}",
                    Constants.UI.ButtonCommands.SET_BIRTH_LOCATION),
                };


                var natalIcon = string.Empty;
                if(Constants.UI.ChartTypeIconDict.TryGetValue(ChartTypeEnum.Natal, out var _natalIcon))
                {
                    natalIcon = _natalIcon;
                }

                var natalCharButton = new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                    $"{natalIcon} {_localeManager.GetString("NatalChartButton", clientData.CultureInfo)}",
                    Constants.UI.ButtonCommands.NATAL_CHART)
                };

                var transitIcon = string.Empty;
                if (Constants.UI.ChartTypeIconDict.TryGetValue(ChartTypeEnum.Transit, out var _transitIcon))
                {
                    transitIcon = _transitIcon;
                }

                var transitForecastButton = new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                    $"{transitIcon} {_localeManager.GetString("TransitForecastButton", clientData.CultureInfo)}",
                    Constants.UI.ButtonCommands.TRANSIT_FORECAST),
                };

                var directionIcon = string.Empty;
                if (Constants.UI.ChartTypeIconDict.TryGetValue(ChartTypeEnum.Direction, out var _directionIcon))
                {
                    directionIcon = _directionIcon;
                }

                var directionForecastButton = new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                    $"{directionIcon} {_localeManager.GetString("DirectionForecastButton", clientData.CultureInfo)}",
                    Constants.UI.ButtonCommands.DIRECTION_FORECAST),
                };

                if (clientData.AstroUser.Longitude == null || clientData.AstroUser.Latitude == null)
                {
                    keyboard = GetKeyboard(
                        setLocationButton,
                        natalCharButton,
                        transitForecastButton, 
                        directionForecastButton);
                }
                else
                {
                    keyboard = GetKeyboard(
                        natalCharButton,
                        transitForecastButton,
                        directionForecastButton);
                }

                //TODO new [] { InlineKeyboardButton.WithCallbackData($"Рассчитать благоприятные дни", Constants.ButtonCommands.POSITIVE_FORECAST) }
            }
            else
            {
                message = $"{Constants.UI.Icons.Common.SCIENCE} {_localeManager.GetString("FillYourBirthDate", clientData.CultureInfo)}.";

                var setBirthDateButton = new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                    _localeManager.GetString("SetBirthDate", clientData.CultureInfo),
                    Constants.UI.ButtonCommands.SET_BIRTHDATE)
                };

                keyboard = GetKeyboard(setBirthDateButton);
            }

            return (message, keyboard);
        }

        private InlineKeyboardMarkup GetKeyboard(params InlineKeyboardButton[][] buttons)
        {
            return new InlineKeyboardMarkup(buttons);
        }

        private (string Message, InlineKeyboardMarkup Keyboard) GetChooseLanguageKeyboard(TBotClientData clientData)
        {
            var message = $"{_localeManager.GetString("ChooseLanguage", clientData.CultureInfo)}:";

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

        public List<string> GetChartMessage(
            List<AspectInfo> aspects, 
            TBotClientData clientData, 
            ChartTypeEnum chartTypeEnum)
        {
            var returnList = new List<string>();

            var icon = string.Empty;

            if (ChartTypeIconDict.TryGetValue(chartTypeEnum, out var _icon))
            {
                icon = _icon;
            }

            var chartType = chartTypeEnum.ToString() + "AspectsInfo";

            returnList.Add($"{icon} " +
                $"{_localeManager.GetString(chartType, clientData.CultureInfo)}: \n" +
                $"{clientData.AstroUser.DateToString(clientData.CultureInfo)}\n\n");

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

                var transitPlanetIcon = PlanetIconDict[aspect.TransitPlanet.Planet];
                var transitZodiacIcon = ZodiacIconDict[aspect.TransitPlanet.Zodiac];

                var natalPlanetIcon = PlanetIconDict[aspect.NatalPlanet.Planet];
                var natalZodiacIcon = ZodiacIconDict[aspect.NatalPlanet.Zodiac];

                var aspectIcon = AspectIconDict[aspect.Aspect];

                var transitRetroIcon = aspect.TransitPlanet.IsRetro ? Icons.Common.RETRO : string.Empty;
                var natalRetroIcon = aspect.NatalPlanet.IsRetro ? Icons.Common.RETRO : string.Empty;

                var transitRetroStr = aspect.TransitPlanet.IsRetro ? "(R)" : string.Empty;
                var natalRetroStr = aspect.NatalPlanet.IsRetro ? "(R)" : string.Empty;

                var transitStr = $"{transitPlanetIcon}{transitRetroIcon}  {transitZodiacIcon}" +
                    $"[{transitAnglesStr}{Icons.Common.ANGLES}{transitMinutesStr}{Icons.Common.MINUTES}]";

                if (chartTypeEnum == ChartTypeEnum.Natal)
                {
                    transitStr += $" ( {aspect.TransitPlanet.StandHouse} )";
                }

                var natalStr = $"{natalPlanetIcon}{natalRetroIcon}  {natalZodiacIcon}" +
                    $"[{natalAnglesStr}{Icons.Common.ANGLES}{natalMinutesStr}{Icons.Common.MINUTES}]";

                if (chartTypeEnum == ChartTypeEnum.Natal)
                {
                    natalStr += $" ( {aspect.NatalPlanet.StandHouse} )";
                }

                sb.Append($"{_localeManager.GetString(aspect.TransitPlanet.Planet.ToString(), clientData.CultureInfo)}" +
                    $"{transitRetroStr} " +
                    $"{Icons.Common.MINUS} " +
                    $"{_localeManager.GetString(aspect.Aspect.ToString(), clientData.CultureInfo)} " +
                    $"{Icons.Common.MINUS} " +
                    $"{_localeManager.GetString(aspect.NatalPlanet.Planet.ToString(), clientData.CultureInfo)}" +
                    $"{natalRetroStr}");

                if (chartTypeEnum == ChartTypeEnum.Transit && 
                    aspect.TransitPlanet.Planet == PlanetEnum.Moon)
                {
                    sb.Append($" [{aspect.StartDate.ToString("HH:mm")} - {aspect.EndDate.ToString("HH:mm")}]");
                }

                sb.Append($"\n{transitStr}   {aspectIcon}   {natalStr}");

                sb.Append("\n");

                var description = GetAspectDescription(aspect, clientData.CultureInfo, chartTypeEnum);

                var expandableDescription = $@"<blockquote expandable>{description}</blockquote> ";

                sb.Append(expandableDescription);
                sb.Append("\n");

                returnList.Add(sb.ToString());
            }

            return returnList;
        }

        public string GetPlanetsInfoMessage(ChartInfo chartInfo, TBotClientData clientData)
        {
            var sb = new StringBuilder();

            sb.Append($"{Icons.Common.WHITE_CIRCLE} " +
                $"{_localeManager.GetString("NatalPlanetsInfo", clientData.CultureInfo)}:\n" +
                $"{clientData.AstroUser.DateToString(clientData.CultureInfo)}\n\n");

            var i = 0;

            foreach (var planet in chartInfo.Planets.Values)
            {
                string anglesStr = (planet.ZodiacAngles.ToString().Length < 2)
                    ? $"0{planet.ZodiacAngles}"
                    : planet.ZodiacAngles.ToString();

                string minutesStr = (planet.ZodiacMinutes.ToString().Length < 2)
                    ? $"0{planet.ZodiacMinutes}"
                    : planet.ZodiacMinutes.ToString();

                var planetIcon = PlanetIconDict[planet.Planet];
                var zodiacIcon = ZodiacIconDict[planet.Zodiac];

                var retroIcon = planet.IsRetro ? Icons.Common.RETRO : string.Empty;
                var retroStr = planet.IsRetro ? " (R)" : string.Empty;

                var planetStr = $"{planetIcon}{retroIcon}  {zodiacIcon}" +
                    $"[{anglesStr}{Icons.Common.ANGLES}{minutesStr}{Icons.Common.MINUTES}]";

                var rulesHouses = string.Join(", ", planet.RulerHouses);

                sb.Append($"{_localeManager.GetString(planet.Planet.ToString(), clientData.CultureInfo)}" +
                    $"{retroStr} " +
                    $"{Icons.Common.MINUS} " +
                    $"{planetStr} " +
                    $"{Icons.Common.MINUS} " +
                    $"( {planet.StandHouse} )" +
                    $"{Icons.Common.MINUS} " +
                    $"[{rulesHouses}]\n");

                i++;

                if (i == 5)
                {
                    sb.Append("\n");
                }
            }

            return sb.ToString();
        }

        public string GetHousesInfoMessage(ChartInfo chartInfo, TBotClientData clientData)
        {
            var sb = new StringBuilder();

            sb.Append($"{Icons.Common.WHITE_CIRCLE} " +
                $"{_localeManager.GetString("NatalHousesInfo", clientData.CultureInfo)}:\n" +
                $"{clientData.AstroUser.DateToString(clientData.CultureInfo)}\n\n");

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

                var zodiacIcon = ZodiacIconDict[house.Value.Zodiac];

                var houseStr = $"{zodiacIcon} " +
                    $"[{anglesStr}{Icons.Common.ANGLES}" +
                    $"{minutesStr}{Icons.Common.MINUTES}" +
                    $"{secondsStr}{Icons.Common.SECONDS}]";

                sb.Append($"{house.Key}" +
                    $"{Icons.Common.MINUS} " +
                    $"{houseStr}\n");

                i++;

                if (i == 3)
                {
                    sb.Append("\n");
                    i = 0;
                }
            }

            return sb.ToString();
        }

        private string GetAspectDescription(AspectInfo aspect, CultureInfo cultureInfo, ChartTypeEnum chartTypeEnum)
        {
            var aspectName =
                chartTypeEnum.ToString() +
                aspect.Aspect.ToString() +
                aspect.NatalPlanet.Planet.ToString();

            if (_localeManager.TryGetString(aspectName, cultureInfo, out var descr1))
            {
                return descr1;
            }

            aspectName =
                chartTypeEnum.ToString() +
                aspect.NatalPlanet.Planet.ToString() +
                aspect.Aspect.ToString() +
                aspect.TransitPlanet.Planet.ToString();

            if (_localeManager.TryGetString(aspectName, cultureInfo, out var descr2))
            {
                return descr2;
            }

            return string.Empty;
        }
    }
}
