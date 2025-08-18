using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace AstroTBotService.Db.Entities
{
    public class AstroPerson : IAstroPerson
    {
        [Key]
        public long? Id { get; set; }

        public DateTime? UtcBirthDate { get; set; }

        public TimeSpan? TimeZoneOffset { get; set; }

        [NotMapped]
        public DateTimeOffset LocalDateTimeOffset
        {
            get
            {
                if (UtcBirthDate.HasValue && TimeZoneOffset.HasValue)
                {
                    var dateTime = new DateTime(
                        UtcBirthDate.Value.Year,
                        UtcBirthDate.Value.Month,
                        UtcBirthDate.Value.Day,
                        UtcBirthDate.Value.Hour,
                        UtcBirthDate.Value.Minute,
                        0,
                        DateTimeKind.Unspecified);

                    return new DateTimeOffset(dateTime, TimeZoneOffset.Value);

                }

                return new DateTimeOffset(DateTime.MinValue, TimeSpan.Zero);
            }
        }


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

        // TODO replace?
        private string DateToString(string dateFormat, CultureInfo cultureInfo)
        {
            if (!UtcBirthDate.HasValue || !TimeZoneOffset.HasValue)
            {
                return string.Empty;
            }

            var dateTimeOffset = LocalDateTimeOffset;

            var timeZoneString = string.Empty;

            var timeZoneSign = dateTimeOffset.Offset >= TimeSpan.Zero ? "+" : "-";

            var hoursStr = Math.Abs(dateTimeOffset.Offset.Hours).ToString();

            if (dateTimeOffset.Offset.Minutes == 0)
            {
                timeZoneString = $"{timeZoneSign}{hoursStr}";
            }
            else
            {
                var minutesStr = Math.Abs(dateTimeOffset.Offset.Minutes).ToString();
                var timeZoneMinutes = minutesStr.Length == 1 ? $"0{minutesStr}" : minutesStr;

                timeZoneString = $"{timeZoneSign}{hoursStr}:{timeZoneMinutes}";
            }

            return $"{dateTimeOffset.DateTime.ToString(dateFormat +
                $"{Constants.UI.Icons.Common.MINUS}  " +
                "HH:mm", cultureInfo)} [UTC{timeZoneString}]";
        }
    }
}
