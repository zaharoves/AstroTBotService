using AstroTBotService.Constans;
using AstroTBotService.Entities;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

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

        public async Task SendMainMenu(ITelegramBotClient botClient, long chatId, bool isBirthdateExist)
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

        //public async Task SendMessageAsync(string rmqMessageId, string messageText)
        //{
        //    var inlineKeyboard = new InlineKeyboardMarkup(new[]
        //    {
        //        new [] { InlineKeyboardButton.WithCallbackData("Рассчитать новую дату", $"setBirthday:") }
        //    });

        //    if (TBotHandler.RmqDict.TryGetValue(rmqMessageId, out var chatId))
        //    {
        //        await _botClient.SendMessage(
        //            chatId: chatId,
        //            text: messageText,
        //            replyMarkup: inlineKeyboard);
        //    }
        //}

        private InlineKeyboardMarkup GetMainMenuKeyboard(bool isKnowBirthday, DatePickerData? datePickerData)
        {
            var mainKeyboard = new InlineKeyboardMarkup();

            if (isKnowBirthday)
            {
                mainKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new [] { InlineKeyboardButton.WithCallbackData($"Изменить дату рождения", Constants.ButtonCommands.SET_BIRTHDAY), },
                    new [] { InlineKeyboardButton.WithCallbackData($"Рассчитать прогноз на сегодня", Constants.ButtonCommands.TODAY_FORECAST), },
                    new [] { InlineKeyboardButton.WithCallbackData($"Рассчитать положительные дни", Constants.ButtonCommands.POSITIVE_FORECAST) }
                });
            }
            else
            {
                mainKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData($"Установить день рождения", Constants.ButtonCommands.SET_BIRTHDAY)
                    }
                });
            }

            return mainKeyboard;
        }
    }
}
