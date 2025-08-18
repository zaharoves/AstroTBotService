using AstroCalculation.Entities;
using AstroCalculation.Enums;
using AstroCalculation.Interfaces;
using System.Runtime.InteropServices;
using TimeZoneConverter;
using GeoTimeZone;

namespace AstroCalculation
{
    public class AstroCalculationService : IAstroCalculationService
    {
        private readonly ICommonHelper _commonHelper;
        private readonly ISwissEphemerisService _swissEphemerisService;

        public AstroCalculationService(
            ICommonHelper commonHelper,
            ISwissEphemerisService swissEphemerisService)
        {
            _commonHelper = commonHelper;
            _swissEphemerisService = swissEphemerisService;
        }

        public DateTimeOffset GetDateTimeOffset(DateTime dateTime, double longitude, double latitude)
        {
            var timeZoneInfo = GetTimeZone(longitude, latitude);

            var offset = timeZoneInfo.GetUtcOffset(dateTime);

            var dateTimeOffset = new DateTimeOffset(dateTime, offset);

            return dateTimeOffset;
        }

        public TimeZoneInfo GetTimeZone(double longitude, double latitude)
        {
            var ianaTimeZoneId = TimeZoneLookup.GetTimeZone(latitude, longitude).Result;

            string systemTimeZoneId;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                systemTimeZoneId = TZConvert.IanaToWindows(ianaTimeZoneId);
            }
            else
            {
                // For Linux and macOS use IANA-id
                systemTimeZoneId = ianaTimeZoneId;
            }

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(systemTimeZoneId);

            return timeZoneInfo;
        }


        public async Task<ChartInfo> GetChartInfo(DateTimeOffset dateTimeOffset, double longitude, double latitude, HouseSystemEnum houseSystem)
        {
            return await _swissEphemerisService.GetChart(dateTimeOffset, longitude, latitude, houseSystem);
        }

        public List<AspectInfo> GetNatalAspects(ChartInfo chartInfo)
        {
            return _commonHelper.GetNatalAspects(chartInfo);
        }

        public async Task<List<AspectInfo>> GetTransitAspects(DateTimeOffset dateTimeOffset, DateTime processDateTime, TimeSpan interval, double longitude, double latitude, HouseSystemEnum houseSystem)
        {
            // TODO CHeck

            // planets aspects (without moon)
            var natalDateTime = dateTimeOffset;
            var transitDateTime = processDateTime;

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

        public async Task<List<AspectInfo>> GetDirectionAspects(DateTimeOffset dateTimeOffset, DateTime processDateTime, double longitude, double latitude, HouseSystemEnum houseSystem)
        {
            // TODO CHeck
            var natalDateTime = dateTimeOffset;
            var directionDateTime = processDateTime;

            var natalChart = await _swissEphemerisService.GetChart(natalDateTime, longitude, latitude, houseSystem);
            var directionChart = GetDirectionChart(natalChart, natalDateTime, directionDateTime);

            var aspects = _commonHelper.GetDirectionAspects(natalChart, directionChart);

            return aspects;
        }

        private ChartInfo GetDirectionChart(ChartInfo natalChart, DateTimeOffset birthDateOffset, DateTimeOffset processDateTimeOffset)
        {
            var directionChart = (ChartInfo)natalChart.Clone();
            // TODO CHeck
            var timeSpan = processDateTimeOffset - birthDateOffset;

            if (timeSpan < TimeSpan.Zero)
            {
                throw new ArgumentException($"Agrument \"{nameof(processDateTimeOffset)}\" less than argument \"{birthDateOffset}\".");
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
