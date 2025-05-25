using AstroTBotService.Entities;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;

namespace AstroTBotService.TBot
{
    public class MainMenuHelper : IMainMenuHelper
    {
        private readonly ITelegramBotClient _botClient;

        public MainMenuHelper(
            ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task SendMainMenu(ITelegramBotClient botClient, long chatId)
        {
            var text = string.Empty;
            InlineKeyboardMarkup keyboard = null;

            //Запрашиваем дату рожднеия
            if (TBotHandler.ChatsDict.TryGetValue(chatId, out var chatInfo) && chatInfo.DatePickerData?.DateTime != null)
            {
                text = string.Format(Constants.MAIN_MENU_MESSAGE_BIRTHDAY, chatInfo.DatePickerData.ToString());
                keyboard = GetMainMenuKeyboard(true, chatInfo.DatePickerData);
            }
            else
            {
                text = Constants.MAIN_MENU_MESSAGE;
                keyboard = GetMainMenuKeyboard(false, null);
            }

            await botClient.SendMessage(
                chatId: chatId,
                text: text,
                replyMarkup: keyboard);
        }

        public static InlineKeyboardButton GetCancelButton(string buttonText = null)
        {
            buttonText = string.IsNullOrWhiteSpace(buttonText) ? "Отмена " : buttonText;
            return InlineKeyboardButton.WithCallbackData($"{buttonText} {Constants.Icons.Common.ORANGE_CIRCLE}", $"{Constants.ButtonCommands.TO_MAIN_MENU}");
        }

        private InlineKeyboardMarkup GetMainMenuKeyboard(bool isKnowBirthday, DatePickerData? datePickerData)
        {
            var mainKeyboard = new InlineKeyboardMarkup();

            if (isKnowBirthday)
            {
                mainKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new [] { InlineKeyboardButton.WithCallbackData($"Изменить дату рождения", Constants.ButtonCommands.SET_BIRTHDAY), },
                    new [] { InlineKeyboardButton.WithCallbackData($"Рассчитать прогноз на сегодня", Constants.ButtonCommands.TODAY_FORECAST), },
                    new [] { InlineKeyboardButton.WithCallbackData($"Рассчитать благоприятные дни", Constants.ButtonCommands.POSITIVE_FORECAST) }
                });
            }
            else
            {
                mainKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData($"Установить дату рождения", Constants.ButtonCommands.SET_BIRTHDAY)
                    }
                });
            }

            return mainKeyboard;
        }

        public async Task SendMessage(long chatId, string message)
        {
            try
            {
                if (message.Length > 4096)
                {
                    message = message.Substring(0, 4096);
                }

                await _botClient.SendMessage(
                    chatId: chatId,
                    text: message);
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                Console.WriteLine($"Ошибка Telegram API при отправке сообщения в чат {chatId}: {ex.Message}");
                Console.WriteLine($"Код ошибки: {ex.ErrorCode}"); // Например, 400 Bad Request, 403 Forbidden
                Console.WriteLine($"Описание ошибки: {ex.Parameters}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла непредвиденная ошибка при отправке сообщения в чат {chatId}: {ex.Message}");
                Console.WriteLine($"Стек вызова: {ex.StackTrace}");
            }

            
        }
    }
}
