using Microsoft.Extensions.Options;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using ProtoBuf;
using AstroTBotService.TBot;
using Telegram.Bot.Polling;
using Telegram.Bot;
using System.Text;
using CommonIcons = AstroTBotService.Constants.Icons.Common;
using AstroTBotService.Enums;


namespace AstroTBotService.RMQ
{
    public class RmqConsumerService : BackgroundService
    {
        private readonly ConnectionFactory _factory;
        private readonly string _queueName;

        private IConnection _connection;
        private IModel _channel;

        private readonly string _exchangeName;
        private readonly string _routingKey;

        private readonly IUpdateHandler _updateHandler;
        private IMainMenuHelper _mainMenuHelper;
        private ITelegramBotClient _botClient;

        public RmqConsumerService(
            IOptions<RmqConfig> rmqConfig,
            IUpdateHandler updateHandler,
            IMainMenuHelper tBotClientHelper,
            ITelegramBotClient botClient)
        {
            _factory = new ConnectionFactory()
            {
                HostName = rmqConfig.Value.HostName,
                Port = rmqConfig.Value.Port,
                UserName = rmqConfig.Value.UserName,
                Password = rmqConfig.Value.Password,
                VirtualHost = rmqConfig.Value.VirtualHost
            };

            _queueName = rmqConfig.Value.QueueName2;
            _exchangeName = "exchangeName";
            _routingKey = "routingKey";

            _updateHandler = updateHandler;
            _mainMenuHelper = tBotClientHelper;
            _botClient = botClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Просто чтобы сервис не завершался сразу
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _connection = _factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Объявляем обменник (если он еще не существует)
                _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Direct, durable: true, autoDelete: false, arguments: null);

                // Объявляем очередь (если она еще не существует)
                _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                // Привязываем очередь к обменнику с ключом маршрутизации
                _channel.QueueBind(queue: _queueName, exchange: _exchangeName, routingKey: _routingKey);

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();

                    RmqMessage2 message = null;

                    using (var memoryStream = new MemoryStream(body))
                    {
                        message = Serializer.Deserialize<RmqMessage2>(memoryStream);

                        Console.WriteLine($" [x] Received (Protobuf): Id={message.Id}");
                    }

                    // Имитация обработки сообщения
                    var sendMessage = ConvertToString(message);
                    Console.WriteLine($" [x] Обработано: {message.Id}");

                    // Подтверждаем получение и обработку сообщения.
                    // Если это не сделать, сообщение вернется в очередь (или уйдет в dead-letter exchange, если настроено)
                    // после того, как consumer "умрет" или потеряет соединение.
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    Console.WriteLine($" [x] Подтверждено: '{message.Id}'");

                    TBotHandler.RmqDict.TryGetValue(message.Id, out long chatId);

                    //TODO рассчитать текст сообщения
                    _mainMenuHelper.SendMessage(chatId, sendMessage);
                };

                // Начинаем потребление сообщений
                _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
            }
            catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
            {
                Console.WriteLine($"Не удалось подключиться к RabbitMQ: {ex.Message}");
                Console.WriteLine($"Убедитесь, что RabbitMQ запущен и доступен по адресу {_factory.HostName}:{_factory.Port}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        private string ConvertToString(RmqMessage2 message)
        {
            var sb = new StringBuilder();

            sb.Append($"Прогноз на {message.DateTime.ToString("dd MMMM yyyyг.")}\n");
            sb.Append($"{new string('-', 50)}\n");

            foreach (var aspect in message.Aspects)
            {
                int transitAngles = (int)Math.Truncate(aspect.TransitZodiacAngles);
                string transitAnglesStr = (transitAngles.ToString().Length < 2) ? $"0{transitAngles}" : transitAngles.ToString();

                int transitMinutes = (int)Math.Truncate((aspect.TransitZodiacAngles - transitAngles) * 100);
                string transitMinutesStr = (transitMinutes.ToString().Length < 2) ? $"0{transitMinutes}" : transitMinutes.ToString();

                int natalAngles = (int)Math.Truncate(aspect.NatalZodiacAngles);
                string natalAnglesStr = (natalAngles.ToString().Length < 2) ? $"0{natalAngles}" : natalAngles.ToString();

                int natalMinutes = (int)Math.Truncate((aspect.NatalZodiacAngles - natalAngles) * 100);
                string natalMinutesStr = (natalMinutes.ToString().Length < 2) ? $"0{natalMinutes}" : natalMinutes.ToString();

                var transitPlanetEnum = (PlanetEnum)Enum.Parse(typeof(PlanetEnum), aspect.TransitPlanet, true);
                var transitPlanetIcon = Constants.PlanetIconDict[transitPlanetEnum];

                var natalPlanetEnum = (PlanetEnum)Enum.Parse(typeof(PlanetEnum), aspect.NatalPlanet, true);
                var natalPlanetIcon = Constants.PlanetIconDict[natalPlanetEnum];

                var transitZodiacEnum = (ZodiacEnum)Enum.Parse(typeof(ZodiacEnum), aspect.TransitZodiac, true);
                var transitZodiacIcon = Constants.ZodiacIconDict[transitZodiacEnum];

                var natalZodiacEnum = (ZodiacEnum)Enum.Parse(typeof(ZodiacEnum), aspect.NatalZodiac, true);
                var natalZodiacIcon = Constants.ZodiacIconDict[natalZodiacEnum];

                var aspectEnum = (AspectEnum)Enum.Parse(typeof(AspectEnum), aspect.Aspect, true);
                var aspectIcon = Constants.AspectIconDict[aspectEnum];

                var transitRetroIcon = aspect.IsTransitRetro ? CommonIcons.RETRO : string.Empty;
                var natalRetroIcon = aspect.IsNatalRetro ? CommonIcons.RETRO : string.Empty;

                var transitRetroStr = aspect.IsTransitRetro ? "(R)" : string.Empty;
                var natalRetroStr = aspect.IsNatalRetro ? "(R)" : string.Empty;

                var transitStr = $"{transitPlanetIcon}{transitRetroIcon}  {transitZodiacIcon}[{transitAnglesStr}{CommonIcons.ANGLES}{transitMinutesStr}{CommonIcons.MINUTES}]";
                var natalStr = $"{natalPlanetIcon}{natalRetroIcon}  {natalZodiacIcon}[{natalAnglesStr}{CommonIcons.ANGLES}{natalMinutesStr}{CommonIcons.MINUTES}]";

                sb.Append($"{aspect.TransitPlanet}{transitRetroStr} {aspect.Aspect} {aspect.NatalPlanet}{natalRetroStr}");
                sb.Append($"\n{transitStr}   {aspectIcon}   {natalStr}");
                sb.Append($"\nSome Description");
                sb.Append($"\n\n");
            }

            return sb.ToString();
        }
    }
}
