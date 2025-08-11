using AstroTBotService.AstroCalculation.Entities;
using AstroTBotService.Common;
using AstroTBotService.Enums;

namespace AstroTBotService.AstroCalculation.Services
{
    public class CalculationService : ICalculationService
    {
        private readonly ICommonHelper _commonHelper;
        private readonly ISwissEphemerisService _swissEphemerisService;
        private readonly ILogger<CalculationService> _logger;

        public CalculationService(
            ICommonHelper commonHelper,
            ISwissEphemerisService swissEphemerisService,
            ILogger<CalculationService> logger)
        {
            _commonHelper = commonHelper;
            _swissEphemerisService = swissEphemerisService;
            _logger = logger;
        }

        public async Task<ChartInfo> GetChartInfo(DateTime dateTime, TimeSpan timeZoneOffset, double longitude, double latitude, HouseSystemEnum houseSystem)
        {
            var birthDateTime = (dateTime - timeZoneOffset);

            return await _swissEphemerisService.GetChart(birthDateTime, longitude, latitude, houseSystem);
        }

        public List<AspectInfo> GetNatalAspects(ChartInfo chartInfo)
        {
            return _commonHelper.GetNatalAspects(chartInfo);
        }

        public async Task<List<AspectInfo>> GetTransitAspects(DateTime birthDate, TimeSpan birthTimeZoneOffset, DateTime processDateTime, TimeSpan interval, double longitude, double latitude, HouseSystemEnum houseSystem)
        {
            //planets aspects (without moon)
            var natalDateTime = (birthDate - birthTimeZoneOffset);
            var transitDateTime = (processDateTime - birthTimeZoneOffset);

            var natalChart = await _swissEphemerisService.GetChart(natalDateTime, longitude, latitude, houseSystem);
            var transitChart = await _swissEphemerisService.GetChart(transitDateTime, longitude, latitude, houseSystem);

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

            //moon aspects
            var startUtcDate = processDateTime;
            var endUtcDate = startUtcDate.AddTicks(interval.Ticks);

            var moonDaysInfo = new Dictionary<DateTime, PlanetInfo>();
            var moonStep = new TimeSpan(1, 0, 0);

            while (startUtcDate < endUtcDate)
            {
                var moonTransit = await _swissEphemerisService.GetPlanetInfo(PlanetEnum.Moon, startUtcDate);
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

        public async Task<List<AspectInfo>> GetDirectionAspects(DateTime birthDate, TimeSpan birthTimeZoneOffset, DateTime processDateTime, double longitude, double latitude, HouseSystemEnum houseSystem)
        {
            var natalDateTime = (birthDate - birthTimeZoneOffset);
            var directionDateTime = (processDateTime - birthTimeZoneOffset);

            var natalChart = await _swissEphemerisService.GetChart(natalDateTime, longitude, latitude, houseSystem);
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
                _logger.LogError($"Get direction chart error. Process datetime ({processDateTime}) less than birth date ({birthDate}).");
                return null;
            }

            var totalYears = (double)timeSpan.Days / 365 +
                (double)timeSpan.Hours / (365 * 24) +
                (double)timeSpan.Minutes / (365 * 24 * 60);

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
    }
}
