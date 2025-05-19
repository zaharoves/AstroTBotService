
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using RabbitMQ.Client;
using System.Globalization;
using System.Text;
using System.Threading;
using AstroTBotService.RMQ;
using AstroTBotService.TBot;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AstroTBotService
{
    public class Program
    {
        private static ITelegramBotClient telegramBotClient = new TelegramBotClient("7633316207:AAER-wgES9FTiehku-TeQy7C2wXSMj1AqZo");

        public static void Main(string[] args)
        {
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




            var builder = Host.CreateDefaultBuilder(args);

            var host = builder.ConfigureServices((hostContext, services) =>
            {
                services.Configure<RmqConfig>(hostContext.Configuration.GetSection("RabbitMq"));
                services.AddSingleton<ITelegramBotClient>(provider => telegramBotClient);
                services.AddSingleton<ITBotClientHelper, TBotClientHelper>();
                services.AddSingleton<IRmqProducer, RmqProducer>();
                services.AddScoped<IUpdateHandler, TBotHandler>();
                services.AddScoped<ITBotDatePicker, TBotDatePicker>();
                services.AddHostedService<RmqConsumerService>();
                services.AddHostedService<TBotService>();
                //services.AddSingleton<ISwissEphemeridService, SwissEphemeridService>();
                //services.AddHostedService<RmqConsumerService>();
                //services.AddHostedService<Worker>();
            })
                .Build();

            host.Run();

            Console.Read();

            //var builder = WebApplication.CreateBuilder(args);

            //// Add services to the container.

            //builder.Services.AddControllers();
            //// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            //builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddSwaggerGen();

            //var app = builder.Build();

            //// Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //    //app.UseSwagger();
            //    //app.UseSwaggerUI();
            //}

            //app.UseHttpsRedirection();

            //app.UseAuthorization();


            //app.MapControllers();

            //app.Run();
        }
        #region ф
        #endregion
    }
}
