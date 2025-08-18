using AstroCalculation.Entities;
using AstroCalculation.Enums;

namespace AstroCalculation.Interfaces
{
    public interface IAstroCalculationService
    {
        DateTimeOffset GetDateTimeOffset(DateTime dateTime, double longitude, double latitude);

        TimeZoneInfo GetTimeZone(double longitude, double latitude);

        Task<ChartInfo> GetChartInfo(DateTimeOffset dateTimeOffset, double logitude, double latitude, HouseSystemEnum houseSystem);

        Task<List<AspectInfo>> GetTransitAspects(DateTimeOffset dateTimeOffset, DateTime processDateTime, TimeSpan interval, double longitude, double latitude, HouseSystemEnum houseSystem);

        Task<List<AspectInfo>> GetDirectionAspects(DateTimeOffset dateTimeOffset, DateTime processDateTime, double longitude, double latitude, HouseSystemEnum houseSystem);

        List<AspectInfo> GetNatalAspects(ChartInfo chartInfo);
    }
}
