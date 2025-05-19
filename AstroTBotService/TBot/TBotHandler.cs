using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Globalization;
using System.Diagnostics;
using Telegram.Bot.Polling;
using AstroTBotService.RMQ;

namespace AstroTBotService.TBot
{
    public class TBotHandler : IUpdateHandler
    {
        private Dictionary<long, (TBotStageEnum Stage, DateTime BirthDate)> _stageDict = new Dictionary<long, (TBotStageEnum, DateTime)>();

        //rmqMessageId, chatId 
        public static Dictionary<string, long> RmqDict = new Dictionary<string, long>();


        //private readonly IServiceProvider _serviceProvider;
        private readonly ITelegramBotClient _botClient;
        private readonly ITBotDatePicker _tBotDatePicker;
        private readonly IRmqProducer _rmqProducer;

        public TBotHandler(
            ITelegramBotClient botClient,
            IRmqProducer rmqProducer,
            //IServiceProvider serviceProvider,
            ITBotDatePicker tBotDatePicker)
        {
            _botClient = botClient;
            _rmqProducer = rmqProducer;
            //_serviceProvider = serviceProvider;
            _tBotDatePicker = tBotDatePicker;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update?.Message != null && update?.Message?.Type == MessageType.Text)
            {
                await HandleMessageAsync(update.Message);
            }
            else if (update?.CallbackQuery != null)
            {
                await HandleCallbackAsync(botClient, update.CallbackQuery);
            }
        }

        private async Task HandleMessageAsync(Message message)
        {
            var chatId = message.Chat.Id;

            if (message.Text.ToLower() == "/start")
            {
                await SendStartMessageAsync(_botClient, message);

                SetChatStage(message.Chat.Id, TBotStageEnum.YearPicker);
                return;
            }

            if (GetChatStage(chatId) == TBotStageEnum.MonthPicker)
            {
                if (int.TryParse(message.Text.ToLower().Trim(), out int year) && year >= 1900 && year <= DateTime.Now.Year)
                {
                    await _tBotDatePicker.SendMonthPicker(_botClient, message.Chat.Id, year, "Выберите месяц Вашего рождения:");
                }
                else
                {
                    var forceReplyMarkup = new ForceReplyMarkup() { Selective = true };

                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: $"Некорректное значение.\nВведите год Вашего рождения (от 1900 до {DateTime.Now.Year}):",
                        replyMarkup: forceReplyMarkup);
                }

                SetChatStage(message.Chat.Id, TBotStageEnum.DayPicker);
                return;
            }
        }

        private async Task HandleCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            if (callbackQuery.Data == null)
            {
                return;
            }

            long chatId = callbackQuery.Message.Chat.Id;
            int messageId = callbackQuery.Message.MessageId;

            if (GetChatStage(chatId) == TBotStageEnum.YearPicker)
            {
                var forceReplyMarkup = new ForceReplyMarkup() { Selective = true };

                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Введите год Вашего рождения",
                    replyMarkup: forceReplyMarkup);

                SetChatStage(callbackQuery.Message.Chat.Id, TBotStageEnum.MonthPicker);
            }
            else if (callbackQuery.Data.StartsWith("dateTimePicker:"))
            {
                if (!_tBotDatePicker.TryParseDateTimePicker(callbackQuery, out int year, out int month, out int day, out int hour, out int minute, out int gmtOffset))
                {
                    return;
                }

                switch (GetChatStage(chatId))
                {
                    case TBotStageEnum.DayPicker:
                        await _tBotDatePicker.SendDayPicker(
                            botClient,
                            callbackQuery,
                            year,
                            month,
                            $"Выберите день Вашего рождения ({CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)} {year} г.)");

                        SetChatStage(callbackQuery.Message.Chat.Id, TBotStageEnum.HourPicker);
                        break;
                    case TBotStageEnum.HourPicker:
                        await _tBotDatePicker.SendHourPicker(
                            botClient,
                            callbackQuery,
                            year,
                            month,
                            day,
                            $"Выберите часы Вашего рождения ({day} {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)} {year} г.)");

                        SetChatStage(callbackQuery.Message.Chat.Id, TBotStageEnum.MinutePicker);
                        break;
                    case TBotStageEnum.MinutePicker:
                        await _tBotDatePicker.SendMinutePicker(
                            botClient,
                            callbackQuery,
                            year,
                            month,
                            day,
                            hour,
                            $"Выберите минуты Вашего рождения ({day} {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)} {year} г. {hour}:XX)");

                        SetChatStage(callbackQuery.Message.Chat.Id, TBotStageEnum.GmtPicker);
                        break;
                    case TBotStageEnum.GmtPicker:
                        var selectedDate = new DateTime(year, month, day, hour, minute, 0);

                        await _tBotDatePicker.SendGmtPicker(
                            botClient,
                            callbackQuery,
                            year,
                            month,
                            day,
                            hour,
                            minute,
                            $"Ваш день рождения: {day} {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)} {year}г. {hour}:{minute}\nВыберите часовой пояс вашего рождения:");

                        SetChatStage(callbackQuery.Message.Chat.Id, TBotStageEnum.ConfirmPicker);
                        SetChatBirthDate(callbackQuery.Message.Chat.Id, selectedDate);
                        break;
                    case TBotStageEnum.ConfirmPicker:
                        var gmtSign = gmtOffset >= 0 ? "+" : "-";

                        await _tBotDatePicker.SendConfirmDate(
                            botClient,
                            callbackQuery,
                            year,
                            month,
                            day,
                            hour,
                            minute,
                            gmtOffset,
                            $"День Вашего рождения: {day} {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)} {year}г. {hour}:{minute}. [GMT{gmtSign}{gmtOffset}]");

                        SetChatStage(callbackQuery.Message.Chat.Id, TBotStageEnum.BeginProcessing);
                        break;
                    default:
                        break;
                }
            }
            else if (GetChatStage(chatId) == TBotStageEnum.YearPicker || callbackQuery.Data.ToLower() == "start:")
            {
                var forceReplyMarkup = new ForceReplyMarkup() { Selective = true };

                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Введите год Вашего рождения",
                    replyMarkup: forceReplyMarkup);

                SetChatStage(callbackQuery.Message.Chat.Id, TBotStageEnum.MonthPicker);
            }
            else if (GetChatStage(chatId) == TBotStageEnum.BeginProcessing)
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "В процессе расчета, подождите...",
                    replyMarkup: null);

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new [] { InlineKeyboardButton.WithCallbackData("Рассчитать новую дату", $"start:") }
                });

                var messageGuid = Guid.NewGuid().ToString();
                var birthDate = GetChatBirthDate(chatId);

                var message = new RmqMessage()
                {
                    Id = messageGuid,
                    BirthDateTime = birthDate,
                    StartDateTime = DateTime.Now.AddDays(-10),
                    EndDateTime = DateTime.Now
                };

                _rmqProducer.SendMessage(messageGuid, message);
                RmqDict.Add(messageGuid, chatId);

                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Посчитано, смотри",
                    replyMarkup: inlineKeyboard);

                SetChatStage(callbackQuery.Message.Chat.Id, TBotStageEnum.EndProcessing);
            }
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            await Console.Out.WriteLineAsync("Возникла ошибка!");
        }

        private async Task SendStartMessageAsync(ITelegramBotClient botClient, Message message)
        {
            var user = message?.From;

            string welcomeText = $"Приветствую вас, {user.FirstName ?? string.Empty} {user.LastName ?? string.Empty}!\n\n{TBotConstants.WelcomeMessage}";

            var inlineKeyboard = InlineKeyboardButton.WithCallbackData("Начать", "start:");

            await botClient.SendMessage(
                chatId: message?.Chat?.Id,
                text: welcomeText,
                replyMarkup: inlineKeyboard);
        }

        private void SetChatStage(long chatId, TBotStageEnum stageEnum)
        {
            if (_stageDict.TryGetValue(chatId, out var chatInfo))
            {
                _stageDict[chatId] = (stageEnum, _stageDict[chatId].BirthDate);
            }
            else
            {
                _stageDict.Add(chatId, (stageEnum, DateTime.MinValue));
            }
        }

        private TBotStageEnum GetChatStage(long chatId)
        {
            if (_stageDict.TryGetValue(chatId, out var chatInfo))
            {
                return chatInfo.Stage;
            }
            else
            {
                return TBotStageEnum.Begin;
            }
        }

        private void SetChatBirthDate(long chatId, DateTime birthDate)
        {
            if (_stageDict.TryGetValue(chatId, out var chatInfo))
            {
                _stageDict[chatId] = (_stageDict[chatId].Stage, birthDate);
            }
            else
            {
                _stageDict.Add(chatId, (TBotStageEnum.Begin, birthDate));
            }
        }

        private DateTime GetChatBirthDate(long chatId)
        {
            if (_stageDict.TryGetValue(chatId, out var chatInfo))
            {
                return chatInfo.BirthDate;
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
