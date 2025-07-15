using AstroTBotService.AstroCalculation.Services;
using AstroTBotService.Configurations;
using AstroTBotService.Db;
using AstroTBotService.Db.Providers;
using AstroTBotService.TBot;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Serilog;
using Serilog.Debugging;


namespace AstroTBotService
{
    public class Program
    {
        private static ITelegramBotClient telegramBotClient = new TelegramBotClient("7633316207:AAER-wgES9FTiehku-TeQy7C2wXSMj1AqZo");

        public static async Task Main(string[] args)
        {
            SelfLog.Enable(Console.Error);

            var builder = Host.CreateDefaultBuilder(args);

            var hostBuilder = builder.ConfigureServices((hostContext, services) =>
            {
                services.Configure<PostgresConfig>(hostContext.Configuration.GetSection(PostgresConfig.ConfigKey));
                services.Configure<AstroConfig>(hostContext.Configuration.GetSection(AstroConfig.ConfigKey));
                services.Configure<SerilogConfig>(hostContext.Configuration.GetSection(SerilogConfig.ConfigKey));

                services.AddSingleton(provider => telegramBotClient);
                services.AddSingleton<IResourcesLocaleManager, ResourcesLocaleManager>();

                services.AddScoped(serviceProvider =>
                {
                    var config = serviceProvider.GetRequiredService<IOptions<PostgresConfig>>();
                    return new ApplicationContext(config);
                });

                services.AddScoped<ICalculationService, CalculationService>();
                services.AddScoped<IUserProvider, UserProvider>();
                services.AddScoped<IEphemerisProvider, EphemerisProvider>();
                services.AddScoped<ISwissEphemerisService, SwissEphemerisService>();

                services.AddScoped<IMainMenuHelper, MainMenuHelper>();
                services.AddScoped<IUpdateHandler, TBotUpdateHandler>();
                services.AddScoped<IDatePicker, DatePicker>();
                services.AddScoped<ILocationPicker, LocationPicker>();

            });

            // 2. Настраиваем Serilog, используя IOptions
            hostBuilder.UseSerilog((context, services, configuration) =>
            {

                // Получаем экземпляр SerilogOptions из DI-контейнера
                var serilogOptions = services.GetRequiredService<IOptions<SerilogConfig>>().Value;

                configuration.WriteTo.PostgreSQL(
                    serilogOptions.ConnectionString,
                    serilogOptions.TableName,
                    needAutoCreateTable: true
                    //columnOptions: columnOptions
                    );

                //if (context.HostingEnvironment.IsProduction() == false)
                //{
                configuration.WriteTo.Console().MinimumLevel.Debug();
                configuration.Enrich.WithProperty("Sample App 1", "Sample App");


                //NEW

                //var columnOptions = new Serilog.Sinks.PostgreSQL.ColumnOptions();
                //columnOptions.CustomColumns.Add("Properties", new Serilog.Sinks.PostgreSQL.ColumnWriters.LogEventSerializedColumnWriter(NpgsqlTypes.NpgsqlDbType.Jsonb));

                //configuration
                //    .ReadFrom.Configuration(context.Configuration) 
                //    .ReadFrom.Services(services)
                //    .Enrich.FromLogContext();
                //.WriteTo.PostgreSQL(
                //    serilogOptions.ConnectionString,
                //    serilogOptions.TableName,
                //    needAutoCreateTable: true,
                //    columnOptions: columnOptions
                //);
                //}
            });

            var host = hostBuilder.Build();

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


            var year = 2020;
            var month = 5;
            var month1 = 4;


            //test
            var a = host.Services.GetRequiredService<ISwissEphemerisService>();
            var b0 = a.GetDataTest(new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc));
            await Console.Out.WriteLineAsync(b0.Info[0].ToString());

            var b1 = a.GetDataTest(new DateTime(year, month1, 30, 23, 0, 0, DateTimeKind.Utc));
            await Console.Out.WriteLineAsync(b1.Info[0].ToString());

            var b2 = a.GetDataTest(new DateTime(year, month, 1, 1, 0, 0, DateTimeKind.Utc));
            await Console.Out.WriteLineAsync(b2.Info[0].ToString());

            var b3 = a.GetDataTest(new DateTime(1995, 2, 15, 5, 27, 0, DateTimeKind.Utc));
            await Console.Out.WriteLineAsync(b3.Info[0].ToString());

            //var b2 = a.GetDataTest(new DateTime(year, month1, 30, 23, 0, 0, DateTimeKind.Utc));
            //await Console.Out.WriteLineAsync(b2.Info[0].ToString());

            //var b21 = a.GetDataTest(new DateTime(year, month1, 30, 23, 20, 0, DateTimeKind.Utc));
            //await Console.Out.WriteLineAsync(b21.Info[0].ToString());

            //var b3 = a.GetDataTest(new DateTime(year, month1, 30, 23, 30, 0, DateTimeKind.Utc));
            //await Console.Out.WriteLineAsync(b3.Info[0].ToString());

            //var b4 = a.GetDataTest(new DateTime(year, month1, 30, 23, 40, 0, DateTimeKind.Utc));
            //await Console.Out.WriteLineAsync(b4.Info[0].ToString());

            //var b5 = a.GetDataTest(new DateTime(year, month1, 30, 23, 50, 0, DateTimeKind.Utc));
            //await Console.Out.WriteLineAsync(b5.Info[0].ToString());

            //var b6 = a.GetDataTest(new DateTime(year, month1, 30, 23, 55, 0, DateTimeKind.Utc));
            //await Console.Out.WriteLineAsync(b6.Info[0].ToString());

            //var b7 = a.GetDataTest(new DateTime(year, month1, 30, 23, 59, 0, DateTimeKind.Utc));
            //await Console.Out.WriteLineAsync(b7.Info[0].ToString());

            //test

            var me = await botClient.GetMe();
            Console.WriteLine($"Start listening for @{me.Username}");

            await host.RunAsync();
        }
    }
}
