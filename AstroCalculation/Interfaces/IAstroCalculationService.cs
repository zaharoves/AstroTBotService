using AstroCalculation.Entities;
using AstroCalculation.Enums;

namespace AstroCalculation.Interfaces
{
    public interface IAstroCalculationService
    {
        Task<ChartInfo> GetChartInfo(DateTime dateTime, TimeSpan timeZoneOffset, double logitude, double latitude, HouseSystemEnum houseSystem);

        Task<List<AspectInfo>> GetTransitAspects(DateTime birthDate, TimeSpan birthTimeZoneOffset, DateTime processDateTime, TimeSpan interval, double longitude, double latitude, HouseSystemEnum houseSystem);

        Task<List<AspectInfo>> GetDirectionAspects(DateTime birthDate, TimeSpan birthTimeZoneOffset, DateTime processDateTime, double longitude, double latitude, HouseSystemEnum houseSystem);

        List<AspectInfo> GetNatalAspects(ChartInfo chartInfo);
    }
}
