using AstroTBotService.AstroCalculation.Entities;
using AstroTBotService.Enums;

namespace AstroTBotService.AstroCalculation.Services
{
    public interface ICalculationService
    {
        Task<ChartInfo> GetChartInfo(DateTime dateTime, TimeSpan gmtOffset, double logitude, double latitude, HouseSystemEnum houseSystem);

        Task<List<AspectInfo>> GetTransitAspects(DateTime birthDate, TimeSpan birthGmtOffset, DateTime processDateTime, TimeSpan interval, double longitude, double latitude, HouseSystemEnum houseSystem);

        Task<List<AspectInfo>> GetDirectionAspects(DateTime birthDate, TimeSpan birthGmtOffset, DateTime processDateTime, double longitude, double latitude, HouseSystemEnum houseSystem);

        Task<List<AspectInfo>> GetNatalAspects(ChartInfo chartInfo);
    }
}
