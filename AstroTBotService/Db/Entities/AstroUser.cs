using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using AstroCalculation.Enums;

namespace AstroTBotService.Db.Entities
{
    public class AstroUser : IAstroPerson
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long? Id { get; set; }

        public string? Language { get; set; }

        [NotMapped]
        public CultureInfo CultureInfo => new CultureInfo(Language);

        public HouseSystemEnum HouseSystem { get; set; } = HouseSystemEnum.Placidus;

        public IList<AstroPerson> ChildPersons { get; set; } = new List<AstroPerson>();

        public bool IsUser => true;

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

                return new DateTimeOffset(DateTime.MinValue, TimeSpan.MinValue);
            }
        }


        public double? Longitude { get; set; }
        public double? Latitude { get; set; }

        public long? ParentUserId { get; set; }
        public AstroUser? ParentUser { get; set; }

        public string? Name { get; set; }
        public bool? IsChosen { get; set; }

        public List<IAstroPerson> GetAllPersons()
        {
            var persons = new List<IAstroPerson>
            {
                this
            };

            persons.AddRange(ChildPersons);

            return persons;
        }

        public IAstroPerson GetChosenPerson()
        {
            var persons = GetAllPersons();

            // TODO
            return persons.FirstOrDefault(p => p.IsChosen == true);
        }

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
