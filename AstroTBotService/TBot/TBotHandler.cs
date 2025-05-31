using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Globalization;
using Telegram.Bot.Polling;
using AstroTBotService.RMQ;
using AstroTBotService.Enums;
using AstroHandlerService.Db.Providers;

namespace AstroTBotService.TBot
{
    public class TBotHandler : IUpdateHandler
    {
        //rmqMessageId, chatId 
        public static Dictionary<string, long> RmqDict = new Dictionary<string, long>();

        private readonly ITelegramBotClient _botClient;
        private readonly IMainMenuHelper _mainMenuHelper;
        private readonly IDatePicker _datePicker;
        private readonly IRmqProducer _rmqProducer;
        private readonly IUserProvider _userProvider;

        public TBotHandler(
            ITelegramBotClient botClient,
            IMainMenuHelper mainMenuHelper,
            IDatePicker tBotDatePicker,
            IRmqProducer rmqProducer,
            IUserProvider userProvider)
        {
            _botClient = botClient;
            _mainMenuHelper = mainMenuHelper;
            _datePicker = tBotDatePicker;
            _rmqProducer = rmqProducer;
            _userProvider = userProvider;
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
                    chatId);

                await _userProvider.SetUserStage(message.Chat.Id, ChatStageEnum.MainMenu.ToString());

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

                    await _userProvider.SetUserStage(chatId, ChatStageEnum.MainMenu.ToString());
                    return;
                case Constants.ButtonCommands.TODAY_FORECAST:
                    var userInfo = await _userProvider.GetUser(chatId);

                    if (userInfo == null)
                    {
                        return;
                    }

                    await botClient.SendMessage(
                            chatId: chatId,
                            text: $"{Constants.Icons.Common.HOURGLASS} Прогноз на сегодня в процессе расчета, пожалуйста подождите...",
                            replyMarkup: null);;

                    var messageGuid = Guid.NewGuid().ToString();

                    var rmqMessage = new UserInfoMessage()
                    { 
                        MessageId =  messageGuid,
                        DateTime = userInfo.BirthDate,
                        GmtOffset = userInfo.GmtOffset
                    };

                    _rmqProducer.SendMessage(messageGuid, rmqMessage);
                    RmqDict.Add(messageGuid, chatId);

                    return;
            }


            #region DatePicker

            _datePicker.TryParseDateTimePicker(callbackQuery, out var datePickerData);

            var userStage = _userProvider.GetUserStage(chatId).Result;
            var userStageEnum = (ChatStageEnum)Enum.Parse(typeof(ChatStageEnum), userStage?.Stage, true);

            switch (userStageEnum)
            {
                case ChatStageEnum.MainMenu:
                    switch (messageData)
                    {
                        case Constants.ButtonCommands.SET_BIRTHDAY:
                            await _datePicker.SendYearIntervalPicker(
                                botClient,
                                callbackQuery,
                                $"Выберите год Вашего рождения:");
                            
                            await _userProvider.SetUserStage(chatId, ChatStageEnum.YearIntervalPicker.ToString());
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

                    await _userProvider.SetUserStage(chatId, ChatStageEnum.YearPicker.ToString());
                    break;

                case ChatStageEnum.YearPicker:
                    await _datePicker.SendMonthPicker(
                        botClient,
                        callbackQuery,
                        datePickerData,
                        "Выберите месяц Вашего рождения:");

                    await _userProvider.SetUserStage(chatId, ChatStageEnum.MonthPicker.ToString());
                    return;

                case ChatStageEnum.MonthPicker:
                    await _datePicker.SendDayPicker(
                        botClient,
                        callbackQuery,
                        datePickerData,
                        $"Выберите день Вашего рождения ({CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(datePickerData?.DateTime?.Month ?? 1)} {datePickerData?.DateTime?.Year} г.)");

                    await _userProvider.SetUserStage(chatId, ChatStageEnum.DayPicker.ToString());
                    break;

                case ChatStageEnum.DayPicker:
                    await _datePicker.SendHourPicker(
                        botClient,
                        callbackQuery,
                        datePickerData,
                        $"Выберите часы Вашего рождения ({datePickerData?.DateTime?.Day} {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(datePickerData?.DateTime?.Month ?? 1)} {datePickerData?.DateTime?.Year} г.)");

                    await _userProvider.SetUserStage(chatId, ChatStageEnum.HourPicker.ToString());
                    return;

                case ChatStageEnum.HourPicker:
                    await _datePicker.SendMinutePicker(
                        botClient,
                        callbackQuery,
                        datePickerData,
                        $"Выберите минуты Вашего рождения ({datePickerData?.DateTime?.Day} {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(datePickerData?.DateTime?.Month ?? 1)} {datePickerData?.DateTime?.Year} г. {datePickerData?.DateTime?.Hour}:XX)");

                    await _userProvider.SetUserStage(chatId, ChatStageEnum.MinutePicker.ToString());
                    break;

                case ChatStageEnum.MinutePicker:
                    await _datePicker.SendTimeZonePicker(
                        botClient,
                        callbackQuery,
                        datePickerData,
                        $"Выберите часовой пояс Вашего рождения ({datePickerData?.DateTime?.Day} {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(datePickerData?.DateTime?.Month ?? 1)} {datePickerData?.DateTime?.Year} г. {datePickerData?.DateTime?.Hour}:{datePickerData?.DateTime?.Minute})");

                    await _userProvider.SetUserStage(chatId, ChatStageEnum.TimeZonePicker.ToString());
                    return;

                case ChatStageEnum.TimeZonePicker:
                    var gmtSign = datePickerData?.GmtOffset >= TimeSpan.Zero ? "+" : "-";

                    await _datePicker.SendConfirmDate(
                        botClient,
                        callbackQuery,
                        datePickerData,
                        $"{Constants.Icons.Common.SCIENCE} Дата Вашего рождения:\n{datePickerData?.ToString()}");

                    await _userProvider.SetUserStage(chatId, ChatStageEnum.ConfirmBirthday.ToString());
                    return;

                case ChatStageEnum.ConfirmBirthday:
                    if (datePickerData?.IsSaveCommand ?? false)
                    {
                        var editUserInfo = new AstroHandlerService.Db.Entities.User()
                        { 
                            BirthDate = datePickerData.DateTime,
                            GmtOffset = datePickerData.GmtOffset
                        };

                        await _userProvider.EditUser(chatId, editUserInfo);

                        await _mainMenuHelper.SendMainMenu(
                            botClient,
                            chatId,
                            callbackQuery.Message.Id);

                        await _userProvider.SetUserStage(chatId, ChatStageEnum.MainMenu.ToString());
                    }
                    else if (datePickerData?.IsChangeCommand ?? false)
                    {
                        await _datePicker.SendYearPicker(
                            botClient,
                            callbackQuery,
                            datePickerData,
                            $"Выберите год Вашего рождения:");

                        await _userProvider.SetUserStage(chatId, ChatStageEnum.YearPicker.ToString());
                    }
                    else if (datePickerData?.IsCancelCommand ?? false)
                    {
                        await _mainMenuHelper.SendMainMenu(
                            botClient,
                            chatId);

                        await _userProvider.SetUserStage(chatId, ChatStageEnum.MainMenu.ToString());
                    }

                    return;
            }

            #endregion
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
