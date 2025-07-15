using AstroTBotService.AstroCalculation.Entities;

namespace AstroTBotService.AstroCalculation.Services
{
    public interface ICalculationService
    {
        Task<ChartInfo> GetChartInfo(DateTime dateTime, TimeSpan gmtOffset, double logitude, double latitude);

        Task<List<AspectInfo>> GetDayTransit(DateTime birthDate, TimeSpan birthGmtOffset, DateTime processDateTime, double longitude, double latitude);

        Task<List<AspectInfo>> GetDirection(DateTime birthDate, TimeSpan birthGmtOffset, DateTime processDateTime, double longitude, double latitude);

        Task<List<AspectInfo>> GetNatalChartAspects(DateTime birthDate, TimeSpan birthGmtOffset, double longitude, double latitude);
        Task<List<AspectInfo>> GetNatalChartAspects(ChartInfo chartInfo);

    }
}
