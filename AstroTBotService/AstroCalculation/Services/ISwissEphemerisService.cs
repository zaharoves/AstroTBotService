
using AstroTBotService.AstroCalculation.Entities;
using AstroTBotService.Enums;

namespace AstroTBotService.AstroCalculation.Services
{
    public interface ISwissEphemerisService
    {
        (double[] Info, int Result) GetDataTest(DateTime dateTime);

        ChartInfo GetChart(DateTime dateTime, double logitude, double latitude, HouseSystemEnum houseSystem, out string error);

        void FillEphemeris(DateTime startDate, DateTime endDate, TimeSpan interval, double logitude, double latitude, HouseSystemEnum houseSystem);

        //List<AspectInfo> GetTransitMoonAspects(ChartInfo natalChart, DateTime startUtcDate, DateTime endUtcDate);

        PlanetInfo GetPlanetInfo(PlanetEnum planetEnum, DateTime dateTime, out string error);

        //PlanetInfo GetPlanetInfo1(PlanetEnum planetEnum, DateTime dateTime, out string error);

    }
}
