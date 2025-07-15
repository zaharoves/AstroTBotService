
using AstroTBotService.AstroCalculation.Entities;
using AstroTBotService.Enums;

namespace AstroTBotService.AstroCalculation.Services
{
    public interface ISwissEphemerisService
    {
        ChartInfo GetChartData(DateTime dateTime, double logitude, double latitude);

        Dictionary<DateTime, ChartInfo> GetData(DateTime startTime, DateTime endTime, TimeSpan interval, double logitude, double latitude);

        PlanetInfo GetDayInfo(PlanetEnum planetEnum, DateTime dateTime, out string error);

        List<AspectInfo> GetAspects(ChartInfo natalChart, ChartInfo transitChart, ChartTypeEnum chartType);

        List<AspectInfo> GetTransitAspects(ChartInfo natalChart, ChartInfo transitChart, params PlanetEnum[] transitPlanets);

        List<AspectInfo> GetNatalAspects(ChartInfo natalChart);


        List<AspectInfo> GetTransitMoonAspects(ChartInfo natalChart, DateTime startUtcDate, DateTime endUtcDate);

        //Dictionary<DateTime, List<AspectInfo>> ProcessAspects0(ChartInfo birthDayInfo, List<ChartInfo> transitList);

        //List<PlanetMain> ProcessAspects(ChartInfo birthDayInfo, List<ChartInfo> transitList);

        void FillEphemeris(DateTime startDate, DateTime endDate, TimeSpan interval, double logitude, double latitude);

        (double[] Info, int Result) GetDataTest(DateTime dateTime);
    }
}
