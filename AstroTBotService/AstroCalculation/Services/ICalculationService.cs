using AstroTBotService.AstroCalculation.Entities;
using AstroTBotService.Enums;

namespace AstroTBotService.AstroCalculation.Services
{
    public interface ICalculationService
    {
        Task<ChartInfo> GetChartInfo(DateTime dateTime, TimeSpan timeZoneOffset, double logitude, double latitude, HouseSystemEnum houseSystem);

        Task<List<AspectInfo>> GetTransitAspects(DateTime birthDate, TimeSpan birthTimeZoneOffset, DateTime processDateTime, TimeSpan interval, double longitude, double latitude, HouseSystemEnum houseSystem);

        Task<List<AspectInfo>> GetDirectionAspects(DateTime birthDate, TimeSpan birthTimeZoneOffset, DateTime processDateTime, double longitude, double latitude, HouseSystemEnum houseSystem);

        List<AspectInfo> GetNatalAspects(ChartInfo chartInfo);
    }
}
