using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace AstroTBotService.Db.Entities
{
    public class AstroPerson : IAstroPerson
    {
        [Key]
        public long? Id { get; set; }

        public DateTime? BirthDate { get; set; }

        public TimeSpan? TimeZoneOffset { get; set; }

        //TODO ADD LOCATION NAME


        public double? Longitude { get; set; }
        public double? Latitude { get; set; }


        public long? ParentUserId { get; set; }

        public AstroUser? ParentUser { get; set; }

        public string? Name { get; set; }
        public bool? IsChosen { get; set; }

        public bool IsUser => false;

        public string DateToLongString(CultureInfo cultureInfo)
        {
            return DateToString("d MMMM yyyy ", cultureInfo);
            
        }

        public string DateToShortString(CultureInfo cultureInfo)
        {
            return DateToString("d.MM.yyyy ", cultureInfo);
        }

        private string DateToString(string dateFormat, CultureInfo cultureInfo)
        {
            if (!BirthDate.HasValue)
            {
                return string.Empty;
            }

            var timeZoneString = string.Empty;

            if (TimeZoneOffset.HasValue)
            {
                var timeZoneSign = TimeZoneOffset.Value >= TimeSpan.Zero ? "+" : "-";

                var hoursStr = Math.Abs(TimeZoneOffset.Value.Hours).ToString();

                if (TimeZoneOffset.Value.Minutes == 0)
                {
                    timeZoneString = $"{timeZoneSign}{hoursStr}";
                }
                else
                {
                    var minutesStr = Math.Abs(TimeZoneOffset.Value.Minutes).ToString();
                    var timeZoneMinutes = minutesStr.Length == 1 ? $"0{minutesStr}" : minutesStr;

                    timeZoneString = $"{timeZoneSign}{hoursStr}:{timeZoneMinutes}";
                }
            }

            return $"{BirthDate.Value.ToString(dateFormat +
                $"{Constants.UI.Icons.Common.MINUS}  " +
                "HH:mm", cultureInfo)} [GMT{timeZoneString}]";
        }
    }
}
