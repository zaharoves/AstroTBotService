
namespace AstroTBotService.Entities
{
    public class RedisPersonData
    {
        public string Name { get; set; }
        public bool IsUser { get; set; }

        public string EditingPersonType { get; set; }
        public long EditingPersonId { get; set; }

        public int StartYearInterval { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }

        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }

        public double? Longitude { get; set; }
        public double? Latitude { get; set; }

        public double? TimeZoneMilliseconds { get; set; }

        public DateTime GetDateTime()
        {
            var year = Year <= 0 ? 1 : Year;
            var month = Month <= 0 ? 1 : Month;
            var day = Day <= 0 ? 1 : Day;

            return new DateTime(year, month, day, Hour, Minute, 0, DateTimeKind.Utc);
        }

        public TimeSpan GetTimeZone()
        {
            if (TimeZoneMilliseconds.HasValue)
            {
                return TimeSpan.FromMilliseconds(TimeZoneMilliseconds.Value);
            }
            else
            {
                return TimeSpan.Zero;
            }
        }
    }
}
