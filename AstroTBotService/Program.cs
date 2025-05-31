using AstroHandlerService.Configurations;
using AstroHandlerService.Db;
using AstroHandlerService.Db.Providers;
using AstroTBotService.RMQ;
using AstroTBotService.TBot;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace AstroTBotService
{
    public class Program
    {
        private static ITelegramBotClient telegramBotClient = new TelegramBotClient("7633316207:AAER-wgES9FTiehku-TeQy7C2wXSMj1AqZo");

        public static void Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args);

            var host = builder.ConfigureServices((hostContext, services) =>
            {
                services.Configure<RmqConfig>(hostContext.Configuration.GetSection("RabbitMq"));
                services.Configure<PostgresConfig>(hostContext.Configuration.GetSection("PostgresConfig"));
                
                services.AddScoped(serviceProvider =>
                {
                    var config = serviceProvider.GetRequiredService<IOptions<PostgresConfig>>();
                    return new ApplicationContext(config);
                });

                services.AddTransient(provider => telegramBotClient);
                services.AddScoped<IMainMenuHelper, MainMenuHelper>();
                services.AddScoped<IUserProvider, UserProvider>();
                services.AddScoped<IRmqProducer, RmqProducer>();
                services.AddScoped<IUpdateHandler, TBotHandler>();
                services.AddScoped<IDatePicker, DatePicker>();
                services.AddHostedService<RmqConsumerService>();
                services.AddHostedService<TBotService>();
            }).Build();

            host.Run();





            ////////////////

            //var botName = bot.GetMe().Result.FirstName;

            //var cts = new CancellationTokenSource();
            //var cancellationToken = cts.Token;

            //var receiverOptions = new ReceiverOptions
            //{
            //    AllowedUpdates = { }, // receive all update types
            //};

            //bot.StartReceiving(
            //    TBotHandler.HandleUpdateAsync,
            //    TBotHandler.HandleErrorAsync,
            //    receiverOptions,
            //    cancellationToken
            //);

            //Console.WriteLine($"Запущен бот {botName}");


            /////////////////

            //var rmqSettings = RmqSettings.CreateDefault();
            //var rmqProducer = new RmqProducer(rmqSettings);

            //var mes1 = new RmqMessage() { Id = Guid.NewGuid().ToString(), BirthDateTime = new DateTime(2025, 12, 1), StartDateTime = new DateTime(2001, 11, 15), EndDateTime = new DateTime(2001, 11, 21) };
            //rmqProducer.SendMessage(mes1.Id, mes1);

            //var mes2 = new RmqMessage() { Id = Guid.NewGuid().ToString(), BirthDateTime = new DateTime(2025, 12, 2), StartDateTime = new DateTime(2001, 11, 15), EndDateTime = new DateTime(2001, 11, 22) };
            //rmqProducer.SendMessage(mes2.Id, mes2);

            //var mes3 = new RmqMessage() { Id = Guid.NewGuid().ToString(), BirthDateTime = new DateTime(2025, 12, 3), StartDateTime = new DateTime(2001, 11, 15), EndDateTime = new DateTime(2001, 11, 23) };
            //rmqProducer.SendMessage(mes3.Id, mes3);


            ////////////////

        }
    }
}
