
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

        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public string? TimeZone { get; set; }

        public string? DateTimeOffsetString { get; set; }

        public DateTimeOffset GetDateTimeOffset()
        {
            if (!string.IsNullOrWhiteSpace(DateTimeOffsetString)
                    && DateTimeOffset.TryParse(DateTimeOffsetString, out var dateTimeOffset))
            {
                return dateTimeOffset;
            }
            else
            {
                return DateTimeOffset.MinValue;
            }
        }
    }
}
