using AstroTBotService.AstroCalculation.Entities;
using AstroTBotService.Enums;
using Telegram.Bot.Types;


namespace AstroTBotService.AstroCalculation.Services
{
    public class CalculationService : ICalculationService
    {
        private readonly ISwissEphemerisService _swissEphemerisService;

        public CalculationService(
            ISwissEphemerisService swissEphemerisService)
        {
            _swissEphemerisService = swissEphemerisService;
        }

        public async Task<ChartInfo> GetChartInfo(DateTime dateTime, TimeSpan gmtOffset, double longitude, double latitude)
        {
            var birthDateTime = (dateTime - gmtOffset);

            return _swissEphemerisService.GetChartData(birthDateTime, longitude, latitude);
        }


        public async Task<List<AspectInfo>> GetDayTransit(DateTime birthDate, TimeSpan birthGmtOffset, DateTime processDateTime, double longitude, double latitude)
        {
            //TODO async
            var aspects = ProcessTodayTransit(birthDate, birthGmtOffset, processDateTime, longitude, latitude);

            return aspects;
        }

        public async Task<List<AspectInfo>> GetDirection(DateTime birthDate, TimeSpan birthGmtOffset, DateTime processDateTime, double longitude, double latitude)
        {
            //TODO async
            //var aspects = ProcessTodayTransit(birthDate, birthGmtOffset, processDateTime, longitude, latitude);

            //planets aspects (without moon)
            var natalDateTime = (birthDate - birthGmtOffset);
            var directionDateTime = (processDateTime - birthGmtOffset);

            //TODO await
            var natalChart = _swissEphemerisService.GetChartData(natalDateTime, longitude, latitude);
            var directionChart = GetDirectionChart(natalChart, natalDateTime, directionDateTime);

            var aspects = _swissEphemerisService.GetAspects(natalChart, directionChart, ChartTypeEnum.Direction);

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


        public async Task<List<AspectInfo>> GetNatalChartAspects(DateTime birthDate, TimeSpan birthGmtOffset, double longitude, double latitude)
        {
            //TODO await
            var birthChart = await GetChartInfo(birthDate, birthGmtOffset, longitude, latitude);

            var aspects = _swissEphemerisService.GetNatalAspects(birthChart);

            return aspects;
        }

        public async Task<List<AspectInfo>> GetNatalChartAspects(ChartInfo chartInfo)
        {
            //TODO async
            return _swissEphemerisService.GetNatalAspects(chartInfo);
        }

        private List<AspectInfo> ProcessTodayTransit(DateTime birthDate, TimeSpan birthGmtOffset, DateTime processDateTime, double longitude, double latitude)
        {
            //planets aspects (without moon)
            var natalDateTime = (birthDate - birthGmtOffset);
            var transitDateTime = (processDateTime - birthGmtOffset);

            //TODO await
            var natalChart = _swissEphemerisService.GetChartData(natalDateTime, longitude, latitude);
            var transitChart = _swissEphemerisService.GetChartData(transitDateTime, longitude, latitude);

            var planetsAspects = _swissEphemerisService.GetTransitAspects(
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
            var endUtcDate = startUtcDate.AddDays(1);

            var moonAspects = _swissEphemerisService.GetTransitMoonAspects(natalChart, startUtcDate, endUtcDate);

            //result aspects
            var resultAspects = new List<AspectInfo>();

            resultAspects.AddRange(moonAspects);
            resultAspects.AddRange(planetsAspects);

            return resultAspects;
        }

        //private DailyForecastMessage ConvertToRmqMessage(string messageId, List<AspectInfo> aspects, DateTime dateTime)
        //{
        //    var rmqMesage = new DailyForecastMessage();

        //    rmqMesage.Id = messageId;
        //    rmqMesage.DateTime = dateTime;

        //    rmqMesage.Aspects = new List<RmqMessageAspect>();

        //    foreach (var aspect in aspects)
        //    {
        //        var rmqMessageAspect = new RmqMessageAspect()
        //        {
        //            NatalPlanet = aspect.NatalPlanet.Planet.ToString(),
        //            NatalZodiac = aspect.NatalPlanet.Zodiac.ToString(),
        //            NatalZodiacAngles = aspect.NatalPlanet.ZodiacAngles,
        //            IsNatalRetro = aspect.NatalPlanet.IsRetro,

        //            TransitPlanet = aspect.TransitPlanet.Planet.ToString(),
        //            TransitZodiac = aspect.TransitPlanet.Zodiac.ToString(),
        //            TransitZodiacAngles = aspect.TransitPlanet.ZodiacAngles,
        //            IsTransitRetro = aspect.TransitPlanet.IsRetro,

        //            Aspect = aspect.Aspect.ToString(),
        //            StartDate = aspect.StartDate,
        //            EndDate = aspect.EndDate
        //        };

        //        rmqMesage.Aspects.Add(rmqMessageAspect);
        //    }

        //    return rmqMesage;
        //}
    }
}
