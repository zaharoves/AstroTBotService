using AstroHandlerService.Configurations;
using AstroHandlerService.Db;
using AstroHandlerService.Db.Providers;
using AstroTBotService.RMQ;
using AstroTBotService.TBot;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace AstroTBotService
{
    public class Program
    {
        private static ITelegramBotClient telegramBotClient = new TelegramBotClient("7633316207:AAER-wgES9FTiehku-TeQy7C2wXSMj1AqZo");

        public static async Task Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args);

            var host = builder.ConfigureServices((hostContext, services) =>
            {
                services.Configure<RmqConfig>(hostContext.Configuration.GetSection("RabbitMq"));
                services.Configure<PostgresConfig>(hostContext.Configuration.GetSection("PostgresConfig"));

                services.AddSingleton(provider => telegramBotClient);
                services.AddSingleton<IResourcesLocaleManager, ResourcesLocaleManager>();

                services.AddScoped(serviceProvider =>
                {
                    var config = serviceProvider.GetRequiredService<IOptions<PostgresConfig>>();
                    return new ApplicationContext(config);
                });

                services.AddScoped<IMainMenuHelper, MainMenuHelper>();
                services.AddScoped<IUserProvider, UserProvider>();
                services.AddScoped<IRmqProducer, RmqProducer>();
                services.AddScoped<IUpdateHandler, TBotUpdateHandler>();
                services.AddScoped<IDatePicker, DatePicker>();

                //services.AddHostedService<RmqConsumerService>();
                //services.AddHostedService<TBotService>();
            }).Build();

            ///
            // Получаем Singleton-экземпляр ITelegramBotClient из корневого провайдера
            var botClient = host.Services.GetRequiredService<ITelegramBotClient>();

            // Запуск Long Polling
            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // Получаем все типы обновлений
            };

            botClient.StartReceiving(
                async (client, update, token) =>
                {
                    using (var scope = host.Services.CreateScope())
                    {
                        var updateHandler = scope.ServiceProvider.GetRequiredService<IUpdateHandler>();
                        await updateHandler.HandleUpdateAsync(botClient, update, token);
                    }
                },
                async (client, exception, token) =>
                {
                    Console.WriteLine($"Polling error: {exception.Message}"); 

                    if (exception is ApiRequestException apiRequestException)
                    {
                        Console.WriteLine($"Telegram API Error: {apiRequestException.ErrorCode} - {apiRequestException.Message}");
                    }

                    await Task.CompletedTask; 
                },
                receiverOptions,
                cts.Token
            );

            var me = await botClient.GetMe();
            Console.WriteLine($"Start listening for @{me.Username}");

            await host.RunAsync();
        }
    }
}
