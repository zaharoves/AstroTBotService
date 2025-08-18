using AstroCalculation.Entities;
using AstroCalculation.Enums;
using AstroCalculation.Constants;
using AstroCalculation.Interfaces;
using SwissEphNet;
using AstroCalculation.Configurations;
using Microsoft.Extensions.Options;

namespace AstroCalculation
{
    public class SwissEphemerisService : ISwissEphemerisService
    {
        // SwissEph.SE_GREG_CAL - указывает на григорианский календарь.
        private readonly int _swissCalendarType = SwissEph.SE_GREG_CAL;
        //SwissEph.SEFLG_TRUEPOS - обеспечивает получение истинного положения, без учета аберрации.
        //SwissEph.SEFLG_SWIEPH - гарантирует использование наиболее точных эфемерид, если они доступны.
        private readonly int _swissPositionType = SwissEph.SEFLG_TRUEPOS | SwissEph.SEFLG_SWIEPH | SwissEph.SEFLG_SPEED;

        private SwissEph _swissEphemeris = new SwissEph();

        private readonly IOptions<AstroCalculationConfig> _configuration;

        public SwissEphemerisService(
            IOptions<AstroCalculationConfig> configuration)
        {
            _configuration = configuration;

            try
            {
                if (string.IsNullOrWhiteSpace(_configuration.Value.SwissEphPath))
                {
                    //TODO
                }
                else
                {
                    //Set eph path 
                    _swissEphemeris.swe_set_ephe_path(_configuration.Value.SwissEphPath);
                    //_swissEphemeris.swe_set_jpl_file("C:\\SWEPH\\EPHE");
                }
            }
            catch (Exception ex)
            {
                //TODO
            }
        }

        public async Task<ChartInfo> GetChart(DateTimeOffset dateTimeOffset, double logitude, double latitude, HouseSystemEnum houseSystem)
        {
            var chart = new ChartInfo(dateTimeOffset);

            // Convert date to Julian date
            double minutePart = (dateTimeOffset.Minute / 60.0);
            double secondPart = (dateTimeOffset.Second / 3600.0);

            var chartTask = await Task.Run(() =>
            {
                double day = _swissEphemeris.swe_julday(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, dateTimeOffset.Hour + minutePart + secondPart, _swissCalendarType);

                //Fill planets info
                foreach (var planetEnum in Enum.GetValues(typeof(PlanetEnum)).Cast<PlanetEnum>())
                {
                    var planetInfo = GetPlanetInfo(planetEnum, day, _swissPositionType);
                    chart.Planets.Add(planetEnum, planetInfo);
                }

                // Houses cuspids (1 - 12)
                double[] cusps = new double[13];

                // ASC, MC and others
                double[] ascmc = new double[10];

                char swissHouseSystem = GetSwissHouseSystem(houseSystem);

                var resultCode = _swissEphemeris.swe_houses(day, latitude, logitude, swissHouseSystem, cusps, ascmc);

                //Error code
                if (resultCode == -1)
                {
                    //TODO
                }
                //Warning codes
                else if (resultCode > 0)
                {
                    //TODO
                }

                //Fill houses info
                foreach (var houseEnum in Enum.GetValues(typeof(HouseEnum)).Cast<HouseEnum>())
                {
                    if (houseEnum == HouseEnum.None)
                    {
                        continue;
                    }

                    var houseInfo = new PositionInfo(cusps[(int)houseEnum]);
                    chart.Houses.Add(houseEnum, houseInfo);
                }

                //Fill planets houses info
                FillPlanetsHousesInfo(chart);

                return chart;
            });

            return chartTask;
        }

        public async Task<PlanetInfo> GetPlanetInfo(PlanetEnum planetEnum, DateTime dateTime)
        {
            // Convert to Juluan date
            double minutePart = (dateTime.Minute / 60.0);
            double secondPart = (dateTime.Second / 3600.0);

            var planetInfo = await Task.Run(() =>
            {
                double day = _swissEphemeris.swe_julday(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour + minutePart + secondPart, _swissCalendarType);

                return GetPlanetInfo(planetEnum, day, _swissPositionType);
            });

            return planetInfo;
        }

        // TODO
        //public async Task FillEphemeris(DateTime startDate, DateTime endDate, TimeSpan interval, double logitude, double latitude, HouseSystemEnum houseSystem)
        //{
        //    var saveDbTimeSpan = new TimeSpan(1, 0, 0, 0);

        //    var startIntervalDate = startDate;
        //    var endIntervalDate = startIntervalDate.Add(saveDbTimeSpan);

        //    while (startIntervalDate < endDate)
        //    {
        //        var charts = await GetCharts(startIntervalDate, endIntervalDate, interval, logitude, latitude, houseSystem);

        //        var ephList = new List<Ephemeris>();

        //        foreach (var chart in charts)
        //        {
        //            var ephDb = new Ephemeris()
        //            {
        //                Id = long.Parse($"{chart.DateTime.ToUniversalTime().ToString("yyyyMMddHHmmss")}"),
        //                DateTime = chart.DateTime.ToUniversalTime(),

        //                SunAngles = chart.ChartInfo.Planets[PlanetEnum.Sun].AbsolutAngles,
        //                MoonAngles = chart.ChartInfo.Planets[PlanetEnum.Moon].AbsolutAngles,

        //                MercuryAngles = chart.ChartInfo.Planets[PlanetEnum.Mercury].AbsolutAngles,
        //                VenusAngles = chart.ChartInfo.Planets[PlanetEnum.Venus].AbsolutAngles,
        //                MarsAngles = chart.ChartInfo.Planets[PlanetEnum.Mars].AbsolutAngles,

        //                JupiterAngles = chart.ChartInfo.Planets[PlanetEnum.Jupiter].AbsolutAngles,
        //                SaturnAngles = chart.ChartInfo.Planets[PlanetEnum.Saturn].AbsolutAngles,

        //                UranAngles = chart.ChartInfo.Planets[PlanetEnum.Uran].AbsolutAngles,
        //                NeptuneAngles = chart.ChartInfo.Planets[PlanetEnum.Neptune].AbsolutAngles,
        //                PlutoAngles = chart.ChartInfo.Planets[PlanetEnum.Pluto].AbsolutAngles
        //            };

        //            ephList.Add(ephDb);
        //        }

        //        await _ephemerisProvider.AddEphemerises(ephList);

        //        startIntervalDate = startIntervalDate.Add(saveDbTimeSpan);
        //        endIntervalDate = endIntervalDate.Add(saveDbTimeSpan);
        //    }
        //}

        private PlanetInfo GetPlanetInfo(PlanetEnum planetEnum, double day, int iflag)
        {
            var info = new double[6];
            var error = string.Empty;

            var result = _swissEphemeris.swe_calc_ut(day, ConvertPlanetToSwiss(planetEnum), iflag, info, ref error);

            if (result != SwissEph.OK || !string.IsNullOrEmpty(error))
            {
                //TODO
            }

            var planetInfo = new PlanetInfo(planetEnum, info[0], info[3]);

            return planetInfo;
        }

        private async Task<List<(DateTime DateTime, ChartInfo ChartInfo)>> GetCharts(DateTime startTime, DateTime endTime, TimeSpan interval, double logitude, double latitude, HouseSystemEnum houseSystem)
        {
            if (startTime >= endTime)
            {
                return new List<(DateTime DateTime, ChartInfo ChartInfo)>();
            }

            var currentTime = startTime;

            var daysInfo = new List<(DateTime, ChartInfo)>();

            while (currentTime < endTime)
            {
                var chartInfo = await GetChart(currentTime, logitude, latitude, houseSystem);

                daysInfo.Add((currentTime, chartInfo));

                currentTime = currentTime.Add(interval);
            }

            _swissEphemeris.swe_close();

            return daysInfo;
        }

        private int ConvertPlanetToSwiss(PlanetEnum planetEnum)
        {
            switch (planetEnum)
            {
                case PlanetEnum.Sun:
                    return SwissEph.SE_SUN;
                case PlanetEnum.Moon:
                    return SwissEph.SE_MOON;
                case PlanetEnum.Mercury:
                    return SwissEph.SE_MERCURY;
                case PlanetEnum.Venus:
                    return SwissEph.SE_VENUS;
                case PlanetEnum.Mars:
                    return SwissEph.SE_MARS;
                case PlanetEnum.Jupiter:
                    return SwissEph.SE_JUPITER;
                case PlanetEnum.Saturn:
                    return SwissEph.SE_SATURN;
                case PlanetEnum.Uran:
                    return SwissEph.SE_URANUS;
                case PlanetEnum.Neptune:
                    return SwissEph.SE_NEPTUNE;
                case PlanetEnum.Pluto:
                    return SwissEph.SE_PLUTO;
                default:
                    //TODO
                    return -1;
            }
        }

        private char GetSwissHouseSystem(HouseSystemEnum houseSystem)
        {
            switch (houseSystem)
            {
                case HouseSystemEnum.Placidus:
                    return 'P';
                case HouseSystemEnum.Porphyry:
                    return 'O';
                case HouseSystemEnum.Koch:
                    return 'K';
                default:
                    //TODO
                    return 'P';
            }
        }

        private void FillPlanetsHousesInfo(ChartInfo chartInfo)
        {
            foreach (var planetInfo in chartInfo.Planets.Values)
            {
                HouseEnum planetHouse = HouseEnum.None;

                foreach (var houseInfo in chartInfo.Houses)
                {
                    if (planetInfo.AbsolutAngles >= houseInfo.Value.AbsolutAngles)
                    {
                        planetHouse = houseInfo.Key;
                    }
                }

                planetInfo.StandHouse = planetHouse;
            }

            var prevHouse = HouseEnum.None;
            var unknownZodiacList = new List<ZodiacEnum>();

            foreach (var zodiacEnum in Enum.GetValues(typeof(ZodiacEnum)).Cast<ZodiacEnum>())
            {
                var planetRuler = Astro.HOUSES_RULER_DICT[zodiacEnum];

                var rulerHouses = new List<HouseEnum>();

                var houses = chartInfo.Houses.Where(h => h.Value.Zodiac == zodiacEnum).ToList();

                if ((houses?.Count() ?? 0) == 0 && prevHouse == HouseEnum.None)
                {
                    unknownZodiacList.Add(zodiacEnum);
                }
                else if ((houses?.Count() ?? 0) == 0 && prevHouse != HouseEnum.None)
                {
                    chartInfo.Planets[planetRuler].RulerHouses.Add(prevHouse);
                }
                else
                {
                    chartInfo.Planets[planetRuler].RulerHouses = houses.Select(h => h.Key).ToList();

                    var maxAngles = houses.Max(h => h.Value.AbsolutAngles);
                    prevHouse = houses.FirstOrDefault(h => h.Value.AbsolutAngles == maxAngles).Key;
                }
            }

            foreach (var unknownZodiac in unknownZodiacList)
            {
                var planetRuler = Astro.HOUSES_RULER_DICT[unknownZodiac];
                chartInfo.Planets[planetRuler].RulerHouses.Add(prevHouse);
            }
        }

        // TODO Delete
        public (double[] Info, int Result) GetDataTest(DateTime dateTime)
        {
            //_swissEphemeris.Hou
            //
            double minutePart = (dateTime.Minute / 60.0);
            double secondPart = (dateTime.Second / 3600.0);
            double day = _swissEphemeris.swe_julday(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour + minutePart + secondPart, _swissCalendarType);

            //double[] dret = ;
            //string err = "";
            //_swissEphemeris.swe_utc_to_jd(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, _swissCalendarType, dret, ref err);

            //houses
            double geohgt = 0;       // Высота над уровнем моря (метры)
                                     // Система домов: 'P' = Placidus
            double geolat = 56.848816; // Широта Санкт-Петербурга (например)
            double geolon = 53.212058; // Долгота Санкт-Петербурга (например)

            double[] cusps = new double[13]; // Куспиды домов (от 1 до 12)
            double[] ascmc = new double[10]; // Asc, MC и другие точки

            char houseSystem = 'P';
            _swissEphemeris.swe_houses(day, geolat, geolon, houseSystem, cusps, ascmc);
            Console.WriteLine($"Placid: 1 house - {cusps[1]}, 2 house - {cusps[2]}, 3 house - {cusps[3]}, 4 house - {cusps[4]}, 5 house - {cusps[5]}, 6 house - {cusps[6]}");

            //houseSystem = 'O';  //Порфирия
            //_swissEphemeris.swe_houses(day, geolat, geolon, houseSystem, cusps, ascmc);
            //Console.WriteLine($"Placid: 1 house - {cusps[1]}, 2 house - {cusps[2]}, 3 house - {cusps[3]}, 4 house - {cusps[4]}, 5 house - {cusps[5]}, 6 house - {cusps[6]}");

            //houseSystem = 'K';  //Кох
            //_swissEphemeris.swe_houses(day, geolat, geolon, houseSystem, cusps, ascmc);
            //Console.WriteLine($"Placid: 1 house - {cusps[1]}, 2 house - {cusps[2]}, 3 house - {cusps[3]}, 4 house - {cusps[4]}, 5 house - {cusps[5]}, 6 house - {cusps[6]}");


            _swissEphemeris.swe_houses_armc(day, geolat, geolon, houseSystem, cusps, ascmc);
            //houses_end


            var info = new double[6];
            var error = string.Empty;
            var result = 0;

            try
            {
                result = _swissEphemeris.swe_calc_ut(day, ConvertPlanetToSwiss(PlanetEnum.Moon), _swissPositionType, info, ref error);
            }
            catch (Exception ex)
            {
                //TODO
                var a = 0;
            }

            return (info, result);
        }
    }
}
