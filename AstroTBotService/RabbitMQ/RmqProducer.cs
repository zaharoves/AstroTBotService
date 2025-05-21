using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using ProtoBuf;

namespace AstroTBotService.RMQ
{
    public class RmqProducer : IRmqProducer
    {
        private readonly ConnectionFactory _factory;
        private readonly string _queueName;

        private readonly string _exchangeName;
        private readonly string _routingKey;

        public RmqProducer(IOptions<RmqConfig> rmqConfig)
        {
            _factory = new ConnectionFactory()
            {
                HostName = rmqConfig.Value.HostName,
                Port = rmqConfig.Value.Port,
                UserName = rmqConfig.Value.UserName,
                Password = rmqConfig.Value.Password,
                VirtualHost = rmqConfig.Value.VirtualHost
            };

            _queueName = rmqConfig.Value.QueueName1;
            _exchangeName = "exchangeName";
            _routingKey = "routingKey";
        }

        //public void SendMessage<T>(string exchangeName, string routingKey, T message) where T : class
        public void SendMessage<T>(string messageId, T message) where T : class
        {
            // Имя хоста RabbitMQ. Если RabbitMQ запущен локально в Docker, как в примере выше,
            // то 'localhost' или 'my-rabbit' (если вы добавили my-rabbit в hosts или используете Docker DNS)
            // Для простоты примера оставим 'localhost'
            try
            {
                using (var connection = _factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    // Объявляем очередь. Если она не существует, она будет создана.
                    // durable: true - очередь сохранится после перезапуска RabbitMQ
                    // exclusive: false - очередь доступна разным соединениям
                    // autoDelete: false - очередь не удалится, когда последний потребитель отпишетсяs
                    channel.QueueDeclare(queue: _queueName,
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    using (var memoryStream = new MemoryStream())
                    {
                        Serializer.Serialize(memoryStream, message);
                        var body = memoryStream.ToArray();

                        // Свойства сообщения
                        // persistent: true - сообщение сохранится на диске и не потеряется при перезапуске RabbitMQ
                        var properties = channel.CreateBasicProperties();
                        properties.Persistent = true;

                        channel.BasicPublish(exchange: string.Empty, // Используем default exchange
                                         routingKey: _queueName, // Имя очереди
                                         basicProperties: properties,
                                         body: body);
                    }

                    Console.WriteLine($"Сообщение от Producer #{messageId} в {DateTime.Now.ToString("HH:mm:ss.fff")} отправлено");
                }
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
        }
    }
}
