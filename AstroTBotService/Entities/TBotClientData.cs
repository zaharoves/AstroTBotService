using AstroTBotService.Db.Entities;
using AstroTBotService.Enums;
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

        public ChatStageEnum ChatStageEnum { get; set; }

        public long AstroUserId
        {
            get
            {
                if (AstroUser?.Id == null)
                {
                    return 0;
                }
                else
                {
                    return AstroUser.Id.Value;
                }
            }
        }

        public Message Message { get; set; }
        public string? CallbackData { get; set; }

        public RedisPersonData RedisPersonData { get; set; }
    }
}
