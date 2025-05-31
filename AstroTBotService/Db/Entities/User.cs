using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AstroHandlerService.Db.Entities
{
    public class User
    {
        public User()
        {
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long? Id { get; set; }
        public DateTime? BirthDate { get; set; }
        public TimeSpan? GmtOffset { get; set; }
        public string? Language { get; set; }

        public string DateToString()
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

            return $"{BirthDate.Value.ToString("d MMMM yyyyг. HH:mm")} [GMT{gmtString}]";
        }
    }
}
