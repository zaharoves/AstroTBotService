using AstroTBotService.Entities;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using static AstroTBotService.Constants;

namespace AstroTBotService.TBot
{
    public class MainMenuHelper : IMainMenuHelper
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IResourcesLocaleManager _localeManager;

        public MainMenuHelper(
            ITelegramBotClient botClient,
            IResourcesLocaleManager localeManager)
        {
            _botClient = botClient;
            _localeManager = localeManager;
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

            await _botClient.EditMessageText(
                chatId: clientData.AstroUser.Id,
                messageId: clientData.Message.Id,
                text: menuInfo.Message,
                replyMarkup: menuInfo.Keyboard);
        }

        public InlineKeyboardButton GetCancelButton(TBotClientData clientData)
        {
            var buttonText = _localeManager.GetString("Cancel", clientData.CultureInfo);

            return InlineKeyboardButton.WithCallbackData($"{buttonText} {Constants.Icons.Common.ORANGE_CIRCLE}", $"{Constants.ButtonCommands.TO_MAIN_MENU}");
        }

        public InlineKeyboardButton GetCancelButton(TBotClientData clientData, string buttonText)
        {
            return InlineKeyboardButton.WithCallbackData($"{buttonText} {Constants.Icons.Common.ORANGE_CIRCLE}", $"{Constants.ButtonCommands.TO_MAIN_MENU}");
        }

        private (string Message, InlineKeyboardMarkup Keyboard) GetMainMenuKeyboard(TBotClientData clientData)
        {
            var message = string.Empty;
            var keyboard = new InlineKeyboardMarkup();

            Constants.FlagsInfoDict.TryGetValue(clientData.AstroUser.Language, out (string Icon, string Description) flagData);

            if (clientData.AstroUser.BirthDate != null)
            {
                message = $"{Icons.Common.SUN} {_localeManager.GetString("YourBirthDate", clientData.CultureInfo)}: \n" +
                    $"{clientData.AstroUser.DateToString(clientData.CultureInfo)}\n\n" +
                    $"{_localeManager.GetString("YouCanCalculate", clientData.CultureInfo)}.";

                keyboard = new InlineKeyboardMarkup(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(
                            _localeManager.GetString("ChangeBirthDate", clientData.CultureInfo),
                            Constants.ButtonCommands.SET_BIRTHDATE),

                        InlineKeyboardButton.WithCallbackData(
                            $"{_localeManager.GetString("Change", clientData.CultureInfo)} {flagData.Icon}",
                            Constants.ButtonCommands.CHANGE_LANGUAGE),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(
                            _localeManager.GetString("ProccessForecast", clientData.CultureInfo),
                            Constants.ButtonCommands.TODAY_FORECAST),
                    },
                    //TODO new [] { InlineKeyboardButton.WithCallbackData($"Рассчитать благоприятные дни", Constants.ButtonCommands.POSITIVE_FORECAST) }
                });
            }
            else
            {
                message = $"{Constants.Icons.Common.SCIENCE} {_localeManager.GetString("FillYourBirthDate", clientData.CultureInfo)}.";

                keyboard = new InlineKeyboardMarkup(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(
                            _localeManager.GetString("SetBirthDate", clientData.CultureInfo),
                            Constants.ButtonCommands.SET_BIRTHDATE)
                    }
                });
            }

            return (message, keyboard);
        }

        private (string Message, InlineKeyboardMarkup Keyboard) GetChooseLanguageKeyboard(TBotClientData clientData)
        {
            var message = $"{_localeManager.GetString("ChooseLanguage", clientData.CultureInfo)}:";

            var buttons = new List<InlineKeyboardButton[]>();

            foreach (var flagInfo in Constants.FlagsInfoDict)
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
                GetCancelButton(clientData)
            ]);

            return (message, new InlineKeyboardMarkup(buttons));
        }
    }
}
