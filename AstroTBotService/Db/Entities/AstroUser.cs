using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Globalization;


namespace AstroTBotService.Db.Entities
{
    public class AstroUser
    {
        public AstroUser()
        {
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long? Id { get; set; }

        public DateTime? BirthDate { get; set; }

        public TimeSpan? GmtOffset { get; set; }
        public string? Language { get; set; }

        public double? Longitude { get; set; }
        public double? Latitude { get; set; }

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
                $"{Constants.UI.Icons.Common.MINUS}  " +
                "HH:mm", cultureInfo)} [GMT{gmtString}]";
        }
    }
}
