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

        public DateTime? BirthDate { get; set ; }
        public TimeSpan? TimeZoneOffset { get; set; }

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

            //TODO
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
