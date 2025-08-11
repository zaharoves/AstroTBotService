using AstroTBotService.AstroCalculation.Services;
using AstroTBotService.Configurations;
using AstroTBotService.Db;
using AstroTBotService.Db.Providers;
using AstroTBotService.TBot;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Serilog;
using Serilog.Debugging;
using AstroTBotService.Common;
using Serilog.Sinks.PostgreSQL;
using NpgsqlTypes;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using AstroTBotService.Redis;

namespace AstroTBotService
{
    public class Program
    {
        private static ITelegramBotClient _telegramBotClient;
        private static IHost host;

        public static async Task Main(string[] args)
        {
            SelfLog.Enable(Console.Error);

            var builder = Host.CreateDefaultBuilder(args);

            var hostBuilder = builder.ConfigureServices((hostContext, services) =>
            {
                services.Configure<TelegramBotConfig>(hostContext.Configuration.GetSection(TelegramBotConfig.ConfigKey));
                services.Configure<PostgresConfig>(hostContext.Configuration.GetSection(PostgresConfig.ConfigKey));
                services.Configure<AstroConfig>(hostContext.Configuration.GetSection(AstroConfig.ConfigKey));
                services.Configure<SerilogConfig>(hostContext.Configuration.GetSection(SerilogConfig.ConfigKey));
                services.Configure<RedisConfig>(hostContext.Configuration.GetSection(RedisConfig.ConfigKey));

                var telegramConfig = hostContext.Configuration.GetSection(TelegramBotConfig.ConfigKey).Get<TelegramBotConfig>();

                if (string.IsNullOrWhiteSpace(telegramConfig?.ApiKey))
                {
                    Console.WriteLine("EMPTY TELEGRAM API KEY!");
                    return;
                }

                _telegramBotClient = new TelegramBotClient(telegramConfig.ApiKey);

                services.AddSingleton(provider => _telegramBotClient);
                services.AddSingleton<IResourcesLocaleManager, ResourcesLocaleManager>();
                services.AddSingleton<ICommonHelper, CommonHelper>();

                services.AddScoped<ICalculationService, CalculationService>();
                services.AddScoped<IUserProvider, UserProvider>();
                services.AddScoped<IEphemerisProvider, EphemerisProvider>();
                services.AddScoped<ISwissEphemerisService, SwissEphemerisService>();

                services.AddScoped<ITClientHelper, TClientHelper>();
                services.AddScoped<IUpdateHandler, TBotUpdateHandler>();
                services.AddScoped<IPersonDataPicker, PersonDataPicker>();

                services.AddDbContext<ApplicationContext>((serviceProvider, optionsBuilder) =>
                {
                    var postgresOptions = serviceProvider.GetRequiredService<IOptions<PostgresConfig>>().Value;
                    optionsBuilder.UseNpgsql(postgresOptions.ConnectionString);

                    optionsBuilder.EnableSensitiveDataLogging();
                });

                var redisConfig = hostContext.Configuration.GetSection(RedisConfig.ConfigKey).Get<RedisConfig>();
                services.AddSingleton<IConnectionMultiplexer>(sp =>
                {
                    return ConnectionMultiplexer.Connect(redisConfig.ConnectionString);
                });

                services.AddTransient(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());
                services.AddSingleton<IRedisService, RedisService>();

                services.AddLogging(configure =>
                {
                    configure.ClearProviders();
                    configure.AddSerilog(dispose: true);
                });

                services.AddHostedService<TBotService>();
            });

            hostBuilder.UseSerilog((context, services, configuration) =>
            {
                var serilogOptions = services.GetRequiredService<IOptions<SerilogConfig>>().Value;

                IDictionary<string, ColumnWriterBase> columnOptions = new Dictionary<string, ColumnWriterBase>
                {
                    {"user_chat_id", new SinglePropertyColumnWriter("user_chat_id", PropertyWriteMethod.ToString, NpgsqlDbType.Text, "l") },
                    {"message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
                    {"level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
                    {"timestamp", new TimestampColumnWriter(NpgsqlDbType.Timestamp) },
                    {"exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
                    {"properties", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) },
                    //{"props_test", new PropertiesColumnWriter(NpgsqlDbType.Jsonb) },
                };

                configuration
                    .Enrich.FromLogContext()
                    //.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Error)
                    .WriteTo.PostgreSQL(
                        serilogOptions.ConnectionString,
                        serilogOptions.TableName,
                        needAutoCreateTable: true,
                        columnOptions: columnOptions
                    );


                //if (context.HostingEnvironment.IsProduction() == false)
            });

            host = hostBuilder.Build();

            //TODO TEST CONNECTIONS


            var me = await _telegramBotClient.GetMe();
            Console.WriteLine($"Start listening for @{me.Username}");

            await host.RunAsync();
        }

        public static async Task Test(IHost host)
        {
            //test
            var year = 2020;
            var month = 5;
            var month1 = 4;


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
        }
    }
}
