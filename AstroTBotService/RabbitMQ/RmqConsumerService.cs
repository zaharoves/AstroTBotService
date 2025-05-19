using Microsoft.Extensions.Options;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using AstroTBotService.RMQ;
using ProtoBuf;
using AstroTBotService.TBot;
using Telegram.Bot.Polling;

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
        private ITBotClientHelper _tBotClientHelper;

        public RmqConsumerService(
            IOptions<RmqConfig> rmqConfig, 
            IUpdateHandler updateHandler,
            ITBotClientHelper tBotClientHelper)        
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
            _tBotClientHelper = tBotClientHelper;
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

                        Console.WriteLine($" [x] Received (Protobuf): Id={message.Id}, BirthDateTime={message.Id}, StartDateTime={message.MessageText}");
                    }

                    // Имитация обработки сообщения
                    ProcessMessage(message);
                    Console.WriteLine($" [x] Обработано: {message.Id}");

                    // Подтверждаем получение и обработку сообщения.
                    // Если это не сделать, сообщение вернется в очередь (или уйдет в dead-letter exchange, если настроено)
                    // после того, как consumer "умрет" или потеряет соединение.
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    Console.WriteLine($" [x] Подтверждено: '{message.Id}'");

                    _tBotClientHelper.SendMessageAsync(message.Id, message.MessageText);
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

        private void ProcessMessage(RmqMessage2 message)
        {
            ///
            
        }
    }
}
