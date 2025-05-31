using AstroHandlerService.Db.Entities;
using AstroHandlerService.Db.Providers;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace AstroTBotService.TBot
{
    public class MainMenuHelper : IMainMenuHelper
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUserProvider _userProvider;

        public MainMenuHelper(
            ITelegramBotClient botClient,
            IUserProvider userProvider)
        {
            _botClient = botClient;
            _userProvider = userProvider;
        }

        public async Task SendMainMenu(ITelegramBotClient botClient, long chatId)
        {
            var userInfo = _userProvider.GetUser(chatId).Result;

            if (userInfo == null)
            {
                userInfo = new User()
                {
                    Id = chatId,
                    BirthDate = null,
                    GmtOffset = null,
                    Language = "RU"
                };

                await _userProvider.AddUser(userInfo);
            }

            var menuInfo = GetMainMenuKeyboard(userInfo, userInfo.BirthDate.HasValue);

            await botClient.SendMessage(
                chatId: chatId,
                text: menuInfo.Message,
                replyMarkup: menuInfo.Keyboard);
        }

        public async Task SendMainMenu(ITelegramBotClient botClient, long chatId, int editMessageId)
        {
            var userInfo = _userProvider.GetUser(chatId).Result;

            if (userInfo == null)
            {
                userInfo = new User()
                {
                    Id = chatId,
                    BirthDate = null,
                    GmtOffset = null,
                    Language = "RU"
                };

                await _userProvider.AddUser(userInfo);
            }

            var menuInfo = GetMainMenuKeyboard(userInfo, userInfo.BirthDate.HasValue);

            await botClient.EditMessageText(
                chatId: chatId,
                messageId: editMessageId,
                text: menuInfo.Message,
                replyMarkup: menuInfo.Keyboard);
        }

        public static InlineKeyboardButton GetCancelButton(string buttonText = null)
        {
            buttonText = string.IsNullOrWhiteSpace(buttonText) ? "Отмена " : buttonText;
            return InlineKeyboardButton.WithCallbackData($"{buttonText} {Constants.Icons.Common.ORANGE_CIRCLE}", $"{Constants.ButtonCommands.TO_MAIN_MENU}");
        }

        private (string Message, InlineKeyboardMarkup Keyboard) GetMainMenuKeyboard(User userInfo, bool isKnowBirthday)
        {
            var message = string.Empty;
            var keyboard = new InlineKeyboardMarkup();

            if (isKnowBirthday)
            {
                message = string.Format(Constants.MAIN_MENU_MESSAGE_BIRTHDAY, userInfo.DateToString());
                keyboard = new InlineKeyboardMarkup(new[]
                {
                    new [] { InlineKeyboardButton.WithCallbackData($"Изменить дату рождения", Constants.ButtonCommands.SET_BIRTHDAY), },
                    new [] { InlineKeyboardButton.WithCallbackData($"Рассчитать прогноз на сегодня", Constants.ButtonCommands.TODAY_FORECAST), },
                    new [] { InlineKeyboardButton.WithCallbackData($"Рассчитать благоприятные дни", Constants.ButtonCommands.POSITIVE_FORECAST) }
                });
            }
            else
            {
                message = Constants.MAIN_MENU_MESSAGE;
                keyboard = new InlineKeyboardMarkup(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData($"Установить дату рождения", Constants.ButtonCommands.SET_BIRTHDAY)
                    }
                });
            }

            return (message, keyboard);
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
