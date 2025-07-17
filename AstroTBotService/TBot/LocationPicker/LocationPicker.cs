using AstroTBotService.Entities;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace AstroTBotService.TBot
{
    public class LocationPicker : ILocationPicker
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ITClientHelper _clientHelper;
        private readonly IResourcesLocaleManager _resourcesLocaleManager;
        private readonly IResourcesLocaleManager _localeManager;

        public LocationPicker(
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

        private readonly char _saparator = '_';

        public bool TryParseLocation(string str, out double longitude, out double latitude)
        {
            longitude = 0.0;
            latitude = 0.0;

            if (string.IsNullOrWhiteSpace(str))
            {
                return false;
            }

            var locationInfo = str.Split(_saparator);

            if (locationInfo.Length != 3)
            {
                return false;
            }

            if (TryParseLongitude(locationInfo[1], out var outLongitude) && TryParseLatitude(locationInfo[2], out var outLatitude))
            {
                longitude = outLongitude; 
                latitude = outLatitude;

                return true;
            }

            return false;
        }

        public bool TryParseLongitude(string str, out double longitude)
        {
            longitude = 0;
            str = str.Trim();

            if (double.TryParse(str, out double outValue)
                && outValue >= -180
                && outValue <= 180)
            {
                longitude = outValue;
                return true;
            }

            return false;
        }

        public bool TryParseLatitude(string str, out double latitude)
        {
            latitude = 0;
            str = str.Trim();

            if (double.TryParse(str, out double outValue)
                && outValue >= -90
                && outValue <= 90)
            {
                latitude = outValue;
                return true;
            }

            return false;
        }

        public async Task SendLocation(TBotClientData clientData, string text)
        {
            // Создаем список рядов кнопок
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    _clientHelper.GetCancelButtonWithEdit(clientData)
                }
            });

            await _botClient.SendMessage(
                chatId: clientData.ChatId,
                text: text,
                replyMarkup: inlineKeyboard);
        }

        public async Task SendConfirmCoordinates(TBotClientData clientData, string text)
        {
            // Создаем список рядов кнопок
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"{_resourcesLocaleManager.GetString("Save", clientData.CultureInfo)} {Constants.UI.Icons.Common.GREEN_CIRCLE}",
                        $"{Constants.UI.ButtonCommands.SAVE_BIRTH_LOCATION}_{clientData.Longitude}_{clientData.Latitude}"),

                    InlineKeyboardButton.WithCallbackData(
                        $"{_resourcesLocaleManager.GetString("Change", clientData.CultureInfo)} {Constants.UI.Icons.Common.YELLOW_CIRCLE}",
                        $"{Constants.UI.ButtonCommands.CHANGE_BIRTH_LOCATION}"),
                },
                new []
                {
                    _clientHelper.GetCancelButtonWithEdit(clientData)
                }
            });

            await _botClient.SendMessage(
                chatId: clientData.ChatId,
                text: text,
                replyMarkup: inlineKeyboard);
        }
    }
}
