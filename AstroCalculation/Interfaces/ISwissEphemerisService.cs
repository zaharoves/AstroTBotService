
using AstroCalculation.Entities;
using AstroCalculation.Enums;

namespace AstroCalculation.Interfaces
{
    public interface ISwissEphemerisService
    {
        Task<ChartInfo> GetChart(DateTime dateTime, double logitude, double latitude, HouseSystemEnum houseSystem);

        //Task FillEphemeris(DateTime startDate, DateTime endDate, TimeSpan interval, double logitude, double latitude, HouseSystemEnum houseSystem);

        Task<PlanetInfo> GetPlanetInfo(PlanetEnum planetEnum, DateTime dateTime);

        //TODO Delete
        (double[] Info, int Result) GetDataTest(DateTime dateTime);
    }
}
