using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Telegram.Bot.Types;
using System.Globalization;
using System.Reflection.Metadata;
using AstroTBotService;

namespace AstroHandlerService.Db.Entities
{
    public class AstroUser
    {
        public AstroUser()
        {
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long? Id { get; set; }

        private DateTime? _birthDate;
        public DateTime? BirthDate
        {
            get
            {
                return _birthDate;
            }

            set
            {
                if (value.HasValue)
                {
                    _birthDate = value.Value.ToUniversalTime();
                }
                else
                {
                    _birthDate = value;
                }
            }
        }

        public TimeSpan? GmtOffset { get; set; }
        public string? Language { get; set; }

        public string DateToString(CultureInfo cultureInfo)
        {
            if (!BirthDate.HasValue)
            {
                return string.Empty;
            }

            var gmtString = string.Empty;

            if (GmtOffset.HasValue)
            {
                var gmtSign = GmtOffset.Value >= TimeSpan.Zero ? "+" : "-";

                var hoursStr = Math.Abs(GmtOffset.Value.Hours).ToString();

                if (GmtOffset.Value.Minutes == 0)
                {
                    gmtString = $"{gmtSign}{hoursStr}";
                }
                else
                {
                    var minutesStr = Math.Abs(GmtOffset.Value.Minutes).ToString();
                    var gmtMinutes = minutesStr.Length == 1 ? $"0{minutesStr}" : minutesStr;

                    gmtString = $"{gmtSign}{hoursStr}:{gmtMinutes}";
                }
            }

            return $"{BirthDate.Value.ToString("d MMMM yyyy " +
                $"{Constants.Icons.Common.MINUS}  " +
                "HH:mm", cultureInfo)} [GMT{gmtString}]";
        }
    }
}
