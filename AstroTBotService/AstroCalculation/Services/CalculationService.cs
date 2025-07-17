using AstroTBotService.AstroCalculation.Entities;
using AstroTBotService.Common;
using AstroTBotService.Enums;

namespace AstroTBotService.AstroCalculation.Services
{
    public class CalculationService : ICalculationService
    {
        private readonly ICommonHelper _commonHelper;
        private readonly ISwissEphemerisService _swissEphemerisService;

        public CalculationService(
            ICommonHelper commonHelper,
            ISwissEphemerisService swissEphemerisService)
        {
            _commonHelper = commonHelper;
            _swissEphemerisService = swissEphemerisService;
        }

        public async Task<ChartInfo> GetChartInfo(DateTime dateTime, TimeSpan gmtOffset, double longitude, double latitude, HouseSystemEnum houseSystem)
        {
            var birthDateTime = (dateTime - gmtOffset);

            return _swissEphemerisService.GetChart(birthDateTime, longitude, latitude, houseSystem, out var error);
        }

        public async Task<List<AspectInfo>> GetDirectionAspects(DateTime birthDate, TimeSpan birthGmtOffset, DateTime processDateTime, double longitude, double latitude, HouseSystemEnum houseSystem)
        {
            var natalDateTime = (birthDate - birthGmtOffset);
            var directionDateTime = (processDateTime - birthGmtOffset);

            //TODO await
            var natalChart = _swissEphemerisService.GetChart(natalDateTime, longitude, latitude, houseSystem, out var error);
            var directionChart = GetDirectionChart(natalChart, natalDateTime, directionDateTime);

            var aspects = _commonHelper.GetDirectionAspects(natalChart, directionChart);

            return aspects;
        }

        private ChartInfo GetDirectionChart(ChartInfo natalChart, DateTime birthDate, DateTime processDateTime)
        {
            var directionChart = (ChartInfo)natalChart.Clone();

            var timeSpan = processDateTime - birthDate;

            if (timeSpan < TimeSpan.Zero)
            {
                //TODO
                return null;
            }

            var totalYears = (double)timeSpan.Days/365 +
                (double)timeSpan.Hours/(365*24) +
                (double)timeSpan.Minutes/(365*24*60);

            var directionPlanets = new Dictionary<PlanetEnum, PlanetInfo>();

            foreach (var planet in directionChart.Planets)
            {
                var directValue = planet.Value.AbsolutAngles + totalYears;

                directValue = directValue > 360 
                    ? directValue - 360 
                    : directValue;

                directionPlanets.Add(planet.Key, new PlanetInfo(planet.Key, directValue));
            }

            directionChart.Planets = directionPlanets;

            var directionHouses = new Dictionary<HouseEnum, PositionInfo>();

            foreach (var house in directionChart.Houses)
            {
                var directValue = house.Value.AbsolutAngles + totalYears;

                directValue = directValue > 360
                    ? directValue - 360
                    : directValue;

                directionHouses.Add(house.Key, new PositionInfo(directValue));
            }

            directionChart.Houses = directionHouses;

            return directionChart;
        }

        public async Task<List<AspectInfo>> GetNatalAspects(ChartInfo chartInfo)
        {
            return _commonHelper.GetNatalAspects(chartInfo);
        }

        public async Task<List<AspectInfo>> GetTransitAspects(DateTime birthDate, TimeSpan birthGmtOffset, DateTime processDateTime, TimeSpan interval, double longitude, double latitude, HouseSystemEnum houseSystem)
        {
            //planets aspects (without moon)
            var natalDateTime = (birthDate - birthGmtOffset);
            var transitDateTime = (processDateTime - birthGmtOffset);

            //TODO await
            var natalChart = _swissEphemerisService.GetChart(natalDateTime, longitude, latitude, houseSystem, out var natalError);
            var transitChart = _swissEphemerisService.GetChart(transitDateTime, longitude, latitude, houseSystem, out var transitError);

            var planetsAspects = _commonHelper.GetTransitAspects(
                natalChart,
                transitChart,
                PlanetEnum.Sun,
                PlanetEnum.Mercury,
                PlanetEnum.Venus,
                PlanetEnum.Mars,
                PlanetEnum.Jupiter,
                PlanetEnum.Saturn,
                PlanetEnum.Uran,
                PlanetEnum.Neptune,
                PlanetEnum.Pluto);

            //calculate moon aspects
            var startUtcDate = processDateTime;
            var endUtcDate = startUtcDate.AddTicks(interval.Ticks);

            var moonDaysInfo = new Dictionary<DateTime, PlanetInfo>();
            var moonStep = new TimeSpan(1, 0, 0);

            while (startUtcDate < endUtcDate)
            {
                var moonTransit = _swissEphemerisService.GetPlanetInfo(PlanetEnum.Moon, startUtcDate, out var error);
                moonDaysInfo.Add(startUtcDate, moonTransit);

                startUtcDate = startUtcDate.AddTicks(moonStep.Ticks);
            }

            var moonAspects = _commonHelper.GetTransitPlanetAspects(natalChart, moonDaysInfo, PlanetEnum.Moon);

            //result aspects
            var resultAspects = new List<AspectInfo>();

            resultAspects.AddRange(moonAspects);
            resultAspects.AddRange(planetsAspects);

            return resultAspects;
        }
    }
}
