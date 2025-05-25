using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Globalization;
using Telegram.Bot.Polling;
using AstroTBotService.RMQ;
using AstroTBotService.Entities;
using AstroTBotService.Enums;

namespace AstroTBotService.TBot
{
    public class TBotHandler : IUpdateHandler
    {
        public static Dictionary<long, (ChatStageEnum Stage, DatePickerData DatePickerData)> ChatsDict = new Dictionary<long, (ChatStageEnum, DatePickerData)>();

        //rmqMessageId, chatId 
        public static Dictionary<string, long> RmqDict = new Dictionary<string, long>();

        private readonly ITelegramBotClient _botClient;
        private readonly IMainMenuHelper _mainMenuHelper;
        private readonly IDatePicker _datePicker;
        private readonly IRmqProducer _rmqProducer;

        public TBotHandler(
            ITelegramBotClient botClient,
            IMainMenuHelper mainMenuHelper,
            IDatePicker tBotDatePicker,
            IRmqProducer rmqProducer)
        {
            _botClient = botClient;
            _mainMenuHelper = mainMenuHelper;
            _datePicker = tBotDatePicker;
            _rmqProducer = rmqProducer;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update?.Message != null && update?.Message?.Type == MessageType.Text)
            {
                await HandleMessageAsync(update);
            }
            else if (update?.CallbackQuery != null)
            {
                await HandleCallbackAsync(botClient, update);
            }
        }

        private async Task HandleMessageAsync(Update update)
        {
            var message = update.Message;

            var chatId = message.Chat.Id;

            if (message.Text.ToLower() == Constants.MessageCommands.START)
            {
                //await SendStartMessageAsync(_botClient, message);
                await _mainMenuHelper.SendMainMenu(
                    _botClient,
                    message.Chat.Id);

                SetChatStage(message.Chat.Id, ChatStageEnum.MainMenu);
                return;
            }
        }

        private async Task HandleCallbackAsync(ITelegramBotClient botClient, Update update)
        {
            var callbackQuery = update.CallbackQuery;

            if (callbackQuery?.Data == null ||
                callbackQuery?.Data == Constants.ButtonCommands.IGNORE ||
                callbackQuery?.Message == null)
            {
                return;
            }

            var chatId = callbackQuery.Message.Chat.Id;
            var messageData = callbackQuery.Data;


            switch (messageData)
            {
                case Constants.ButtonCommands.TO_MAIN_MENU:
                    await _mainMenuHelper.SendMainMenu(
                        botClient,
                        chatId);

                    SetChatStage(chatId, ChatStageEnum.MainMenu);
                    return;
                case Constants.ButtonCommands.TODAY_FORECAST:
                    await botClient.SendMessage(
                            chatId: chatId,
                            text: "В процессе расчета, подождите...",
                            replyMarkup: null);

                    var keyboard = new InlineKeyboardMarkup(new[]
                    {
                        new [] { MainMenuHelper.GetCancelButton("На главную") }
                    });

                    var messageGuid = Guid.NewGuid().ToString();

                    if (!ChatsDict.TryGetValue(chatId, out var chatInfo))
                    {
                        return;
                    }

                    var message = new RmqMessage()
                    { 
                        MessageId =  messageGuid,
                        DatePickerData = chatInfo.DatePickerData
                    };

                    _rmqProducer.SendMessage(messageGuid, message);
                    RmqDict.Add(messageGuid, chatId);

                    await botClient.SendMessage(
                        chatId: chatId,
                        text: "Посчитано, смотри",
                        replyMarkup: keyboard);

                    SetChatStage(chatId, ChatStageEnum.ProcessingResult);

                    return;
            }


            #region DatePicker

            _datePicker.TryParseDateTimePicker(callbackQuery, out var datePickerData);

            switch (GetChatStage(chatId))
            {
                case ChatStageEnum.MainMenu:
                    switch (messageData)
                    {
                        case Constants.ButtonCommands.SET_BIRTHDAY:
                            await _datePicker.SendYearIntervalPicker(
                                botClient,
                                callbackQuery,
                                $"Выберите год Вашего рождения:");

                            SetChatStage(chatId, ChatStageEnum.YearIntervalPicker);
                            break;
                        case Constants.ButtonCommands.TODAY_FORECAST:
                            break;
                        case Constants.ButtonCommands.POSITIVE_FORECAST:
                            break;
                    }

                    return;

                case ChatStageEnum.YearIntervalPicker:
                    await _datePicker.SendYearPicker(
                        botClient,
                        callbackQuery,
                        datePickerData,
                        $"Выберите год Вашего рождения:");

                    SetChatStage(chatId, ChatStageEnum.YearPicker);
                    break;

                case ChatStageEnum.YearPicker:
                    await _datePicker.SendMonthPicker(
                        botClient,
                        callbackQuery,
                        datePickerData,
                        "Выберите месяц Вашего рождения:");

                    SetChatStage(chatId, ChatStageEnum.MonthPicker);
                    return;

                case ChatStageEnum.MonthPicker:
                    await _datePicker.SendDayPicker(
                        botClient,
                        callbackQuery,
                        datePickerData,
                        $"Выберите день Вашего рождения ({CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(datePickerData?.DateTime?.Month ?? 1)} {datePickerData?.DateTime?.Year} г.)");

                    SetChatStage(chatId, ChatStageEnum.DayPicker);
                    break;

                case ChatStageEnum.DayPicker:
                    await _datePicker.SendHourPicker(
                        botClient,
                        callbackQuery,
                        datePickerData,
                        $"Выберите часы Вашего рождения ({datePickerData?.DateTime?.Day} {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(datePickerData?.DateTime?.Month ?? 1)} {datePickerData?.DateTime?.Year} г.)");

                    SetChatStage(chatId, ChatStageEnum.HourPicker);
                    return;

                case ChatStageEnum.HourPicker:
                    await _datePicker.SendMinutePicker(
                        botClient,
                        callbackQuery,
                        datePickerData,
                        $"Выберите минуты Вашего рождения ({datePickerData?.DateTime?.Day} {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(datePickerData?.DateTime?.Month ?? 1)} {datePickerData?.DateTime?.Year} г. {datePickerData?.DateTime?.Hour}:XX)");

                    SetChatStage(chatId, ChatStageEnum.MinutePicker);
                    break;

                case ChatStageEnum.MinutePicker:
                    await _datePicker.SendTimeZonePicker(
                        botClient,
                        callbackQuery,
                        datePickerData,
                        $"Выберите часовой пояс Вашего рождения ({datePickerData?.DateTime?.Day} {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(datePickerData?.DateTime?.Month ?? 1)} {datePickerData?.DateTime?.Year} г. {datePickerData?.DateTime?.Hour}:{datePickerData?.DateTime?.Minute})");

                    SetChatStage(chatId, ChatStageEnum.TimeZonePicker);
                    return;

                case ChatStageEnum.TimeZonePicker:
                    var gmtSign = datePickerData?.GmtOffset >= TimeSpan.Zero ? "+" : "-";

                    await _datePicker.SendConfirmDate(
                        botClient,
                        callbackQuery,
                        datePickerData,
                        $"{Constants.Icons.Common.SUN} Дата Вашего рождения:\n{datePickerData?.ToString()}");

                    SetChatStage(chatId, ChatStageEnum.ConfirmBirthday);
                    return;

                case ChatStageEnum.ConfirmBirthday:
                    if (datePickerData?.IsSaveCommand ?? false)
                    {
                        SetDatePickerData(chatId, datePickerData);

                        await _mainMenuHelper.SendMainMenu(
                            botClient,
                            chatId);

                        SetChatStage(chatId, ChatStageEnum.MainMenu);
                    }
                    else if (datePickerData?.IsChangeCommand ?? false)
                    {
                        await _datePicker.SendYearPicker(
                            botClient,
                            callbackQuery,
                            datePickerData,
                            $"Выберите год Вашего рождения:");

                        SetChatStage(chatId, ChatStageEnum.YearPicker);
                    }
                    else if (datePickerData?.IsCancelCommand ?? false)
                    {
                        await _mainMenuHelper.SendMainMenu(
                            botClient,
                            chatId);

                        SetChatStage(chatId, ChatStageEnum.MainMenu);
                    }

                    return;
            }

            #endregion



            //if (GetChatStage(chatId) == TBotStageEnum.Confirm)
            //{
            //    if (messageText == "process:")
            //    {
            //        await botClient.SendMessage(
            //        chatId: chatId,
            //        text: "В процессе расчета, подождите...",
            //        replyMarkup: null);

            //        var inlineKeyboard = new InlineKeyboardMarkup(new[]
            //        {
            //            new [] { InlineKeyboardButton.WithCallbackData("Рассчитать новую дату", $"start:") }
            //        });

            //        var messageGuid = Guid.NewGuid().ToString();
            //        var birthDate = GetChatBirthDate(chatId);

            //        var message = new RmqMessage()
            //        {
            //            Id = messageGuid,
            //            BirthDateTime = birthDate,
            //            StartDateTime = DateTime.Now.AddDays(-10),
            //            EndDateTime = DateTime.Now
            //        };

            //        _rmqProducer.SendMessage(messageGuid, message);
            //        RmqDict.Add(messageGuid, chatId);

            //        await botClient.SendMessage(
            //            chatId: chatId,
            //            text: "Посчитано, смотри",
            //            replyMarkup: inlineKeyboard);

            //        SetChatStage(chatId, TBotStageEnum.EndProcessing);
            //    }

        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            await Console.Out.WriteLineAsync("Возникла ошибка!");
        }

        private async Task SendStartMessageAsync(ITelegramBotClient botClient, Message message)
        {
            var user = message?.From;

            string welcomeText = $"Приветствую вас, {user.FirstName ?? string.Empty} {user.LastName ?? string.Empty}!\n\n{Constants.WELCOME_MESSAGE}";

            var inlineKeyboard = InlineKeyboardButton.WithCallbackData("Ввести дату рождения", Constants.ButtonCommands.SET_BIRTHDAY);

            await botClient.SendMessage(
                chatId: message?.Chat?.Id,
                text: welcomeText,
                replyMarkup: inlineKeyboard);
        }

        private void SetChatStage(long chatId, ChatStageEnum stageEnum)
        {
            if (ChatsDict.TryGetValue(chatId, out var outDatePickerData))
            {
                ChatsDict[chatId] = (stageEnum, ChatsDict[chatId].DatePickerData);
            }
            else
            {
                ChatsDict.Add(chatId, (stageEnum, new DatePickerData()));
            }
        }

        private ChatStageEnum GetChatStage(long chatId)
        {
            if (ChatsDict.TryGetValue(chatId, out var outDatePickerData))
            {
                return outDatePickerData.Stage;
            }
            else
            {
                return ChatStageEnum.MainMenu;
            }
        }

        private void SetDatePickerData(long chatId, DatePickerData datePickerData)
        {
            if (ChatsDict.TryGetValue(chatId, out var outDatePickerData))
            {
                ChatsDict[chatId] = (ChatsDict[chatId].Stage, datePickerData);
            }
            else
            {
                ChatsDict.Add(chatId, (ChatStageEnum.MainMenu, datePickerData));
            }
        }

        private DatePickerData GetDatePickerData(long chatId)
        {
            if (ChatsDict.TryGetValue(chatId, out var outDatePickerData))
            {
                return outDatePickerData.DatePickerData;
            }
            else
            {
                return new DatePickerData();
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
