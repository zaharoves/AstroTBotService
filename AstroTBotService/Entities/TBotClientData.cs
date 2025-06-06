using AstroHandlerService.Db.Entities;
using AstroTBotService.TBot;
using System.Globalization;
using Telegram.Bot.Types;

namespace AstroTBotService.Entities
{
    public class TBotClientData
    {
        public TBotClientData() 
        { 
        }

        public AstroUser AstroUser { get; set; }

        public long ChatId => AstroUser.Id.Value;

        public CultureInfo CultureInfo { get; set; }
        public Message Message { get; set; }
        public string? CallbackData { get; set; }

        public DatePickerData DatePickerData { get; set; } 
    }
}
