using AstroTBotService.AstroCalculation.Entities;
using AstroTBotService.Configurations;
using AstroTBotService.Db.Entities;
using AstroTBotService.Db.Providers;
using AstroTBotService.Enums;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.Extensions.Options;
using SwissEphNet;
using Telegram.Bot.Types.Enums;
using static AstroTBotService.Constants.UI.Icons;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AstroTBotService.AstroCalculation.Services
{
    public class SwissEphemerisService : ISwissEphemerisService
    {
        // SwissEph.SE_GREG_CAL - указывает на григорианский календарь.
        private readonly int _swissCalendarType = SwissEph.SE_GREG_CAL;
        //SwissEph.SEFLG_TRUEPOS - обеспечивает получение истинного положения, без учета аберрации.
        //SwissEph.SEFLG_SWIEPH - гарантирует использование наиболее точных эфемерид, если они доступны.
        private readonly int _swissPositionType = SwissEph.SEFLG_TRUEPOS | SwissEph.SEFLG_SWIEPH | SwissEph.SEFLG_SPEED; //SwissEph.SEFLG_TRUEPOS

        private SwissEph _swissEphemeris = new SwissEph();
        private readonly IEphemerisProvider _ephemerisProvider;

        private readonly Dictionary<PlanetEnum, AspectOrbDictionary> _natalOrbDictionary;
        private readonly Dictionary<PlanetEnum, AspectOrbDictionary> _transitOrbDictionary;
        private readonly Dictionary<PlanetEnum, AspectOrbDictionary> _directionOrbDictionary;

        public SwissEphemerisService(
            IOptions<AstroConfig> astroConfiguration,
            IEphemerisProvider ephemerisProvider)
        {
            //Установка пути к файлам эфемерид
            //SwissEph sw = new SwissEph("C\\:\\ephe");
            try
            {
                _swissEphemeris.swe_set_ephe_path("C:\\SWEPH\\EPHE");
                _swissEphemeris.swe_set_jpl_file("C:\\SWEPH\\EPHE");
            }
            catch
            {
                var a = 1;
            }

            _natalOrbDictionary = new Dictionary<PlanetEnum, AspectOrbDictionary>()
            {
                {PlanetEnum.Sun, new AspectOrbDictionary(astroConfiguration.Value.NatalOrbs.Sun)},
                {PlanetEnum.Moon, new AspectOrbDictionary(astroConfiguration.Value.NatalOrbs.Moon)},

                {PlanetEnum.Mercury, new AspectOrbDictionary(astroConfiguration.Value.NatalOrbs.Mercury)},
                {PlanetEnum.Venus, new AspectOrbDictionary(astroConfiguration.Value.NatalOrbs.Venus)},
                {PlanetEnum.Mars, new AspectOrbDictionary(astroConfiguration.Value.NatalOrbs.Mars)},

                {PlanetEnum.Jupiter, new AspectOrbDictionary(astroConfiguration.Value.NatalOrbs.Jupiter)},
                {PlanetEnum.Saturn, new AspectOrbDictionary(astroConfiguration.Value.NatalOrbs.Saturn)},

                {PlanetEnum.Uran, new AspectOrbDictionary(astroConfiguration.Value.NatalOrbs.Uran)},
                {PlanetEnum.Neptune, new AspectOrbDictionary(astroConfiguration.Value.NatalOrbs.Neptune)},
                {PlanetEnum.Pluto, new AspectOrbDictionary(astroConfiguration.Value.NatalOrbs.Pluto)}
            };

            _transitOrbDictionary = new Dictionary<PlanetEnum, AspectOrbDictionary>()
            {
                {PlanetEnum.Sun, new AspectOrbDictionary(astroConfiguration.Value.TransitOrbs.Sun)},
                {PlanetEnum.Moon, new AspectOrbDictionary(astroConfiguration.Value.TransitOrbs.Moon)},

                {PlanetEnum.Mercury, new AspectOrbDictionary(astroConfiguration.Value.TransitOrbs.Mercury)},
                {PlanetEnum.Venus, new AspectOrbDictionary(astroConfiguration.Value.TransitOrbs.Venus)},
                {PlanetEnum.Mars, new AspectOrbDictionary(astroConfiguration.Value.TransitOrbs.Mars)},

                {PlanetEnum.Jupiter, new AspectOrbDictionary(astroConfiguration.Value.TransitOrbs.Jupiter)},
                {PlanetEnum.Saturn, new AspectOrbDictionary(astroConfiguration.Value.TransitOrbs.Saturn)},

                {PlanetEnum.Uran, new AspectOrbDictionary(astroConfiguration.Value.TransitOrbs.Uran)},
                {PlanetEnum.Neptune, new AspectOrbDictionary(astroConfiguration.Value.TransitOrbs.Neptune)},
                {PlanetEnum.Pluto, new AspectOrbDictionary(astroConfiguration.Value.TransitOrbs.Pluto)}
            };

            _directionOrbDictionary = new Dictionary<PlanetEnum, AspectOrbDictionary>()
            {
                {PlanetEnum.Sun, new AspectOrbDictionary(astroConfiguration.Value.DirectionOrbs.Sun)},
                {PlanetEnum.Moon, new AspectOrbDictionary(astroConfiguration.Value.DirectionOrbs.Moon)},

                {PlanetEnum.Mercury, new AspectOrbDictionary(astroConfiguration.Value.DirectionOrbs.Mercury)},
                {PlanetEnum.Venus, new AspectOrbDictionary(astroConfiguration.Value.DirectionOrbs.Venus)},
                {PlanetEnum.Mars, new AspectOrbDictionary(astroConfiguration.Value.DirectionOrbs.Mars)},

                {PlanetEnum.Jupiter, new AspectOrbDictionary(astroConfiguration.Value.DirectionOrbs.Jupiter)},
                {PlanetEnum.Saturn, new AspectOrbDictionary(astroConfiguration.Value.DirectionOrbs.Saturn)},

                {PlanetEnum.Uran, new AspectOrbDictionary(astroConfiguration.Value.DirectionOrbs.Uran)},
                {PlanetEnum.Neptune, new AspectOrbDictionary(astroConfiguration.Value.DirectionOrbs.Neptune)},
                {PlanetEnum.Pluto, new AspectOrbDictionary(astroConfiguration.Value.DirectionOrbs.Pluto)}
            };

            _ephemerisProvider = ephemerisProvider;
        }

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

        public ChartInfo GetChartData(DateTime dateTime, double logitude, double latitude)
        {
            return GetDayInfo(dateTime, logitude, latitude, out var error);
        }

        public List<AspectInfo> GetAspects(ChartInfo natalChart, ChartInfo transitChart, ChartTypeEnum chartType)
        {
            var aspects = new List<AspectInfo>();

            foreach (var natalPlanet in natalChart.Planets.Values)
            {
                foreach (var transitPlanet in transitChart.Planets.Values)
                {
                    var aspectInfo = GetAspect(natalPlanet, transitPlanet, chartType);

                    if (aspectInfo.Aspect != AspectEnum.None)
                    {
                        aspects.Add(aspectInfo);
                    }

                }
            }

            return aspects;
        }

        public List<AspectInfo> GetTransitAspects(ChartInfo natalChart, ChartInfo transitChart, params PlanetEnum[] transitPlanets)
        {
            var aspects = new List<AspectInfo>();

            foreach (var transitPlanet in transitChart.Planets.Values)
            {
                foreach (var natalPlanet in natalChart.Planets.Values)
                {

                    if (!transitPlanets.Contains(transitPlanet.Planet))
                    {
                        continue;
                    }
                    var aspectInfo = GetAspect(natalPlanet, transitPlanet, ChartTypeEnum.Transit);

                    if (aspectInfo.Aspect != AspectEnum.None)
                    {
                        aspects.Add(aspectInfo);
                    }

                }
            }

            return aspects;
        }

        public List<AspectInfo> GetNatalAspects(ChartInfo natalChart)
        {
            var processedPlanetList = new List<PlanetEnum>();

            var aspects = new List<AspectInfo>();

            foreach (var natalPlanet in natalChart.Planets.Values)
            {
                foreach (var processPlanet in natalChart.Planets.Values)
                {
                    if (natalPlanet.Planet == processPlanet.Planet
                        || processedPlanetList.Contains(processPlanet.Planet))
                    {
                        continue;
                    }

                    var aspectInfo = GetAspect(natalPlanet, processPlanet, ChartTypeEnum.Natal);

                    if (aspectInfo.Aspect != AspectEnum.None)
                    {
                        aspects.Add(aspectInfo);
                    }

                }

                processedPlanetList.Add(natalPlanet.Planet);
            }

            return aspects;
        }

        public AspectInfo GetAspect(PlanetInfo natalPlanet, PlanetInfo transitPlanet, ChartTypeEnum chartType)
        {
            var ordDictionary =  new Dictionary<PlanetEnum, AspectOrbDictionary>();

            switch (chartType)
            {
                case ChartTypeEnum.Natal:
                    ordDictionary = _natalOrbDictionary;
                    break;
                case ChartTypeEnum.Transit:
                    ordDictionary = _transitOrbDictionary;
                    break;
                case ChartTypeEnum.Direction:
                    ordDictionary = _directionOrbDictionary;
                    break;
            }

            if (!ordDictionary.TryGetValue(natalPlanet.Planet, out var aspectsOrb))
            {
                return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.None);
            }

            var angles = Math.Abs(natalPlanet.AbsolutAngles - transitPlanet.AbsolutAngles);

            if (angles >= aspectsOrb[AspectEnum.Conjunction].Min && angles <= Constants.CIRCLE_ANGLES ||
               angles <= aspectsOrb[AspectEnum.Conjunction].Max && angles >= Constants.ZODIAC_ZERO)
            {
                return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.Conjunction);
            }
            else if (angles >= aspectsOrb[AspectEnum.Sextile].Min && angles <= aspectsOrb[AspectEnum.Sextile].Max ||
                angles >= (Constants.CIRCLE_ANGLES - aspectsOrb[AspectEnum.Sextile].Max) && angles <= (Constants.CIRCLE_ANGLES - aspectsOrb[AspectEnum.Sextile].Min))
            {
                return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.Sextile);
            }
            else if (angles >= aspectsOrb[AspectEnum.Square].Min && angles <= aspectsOrb[AspectEnum.Square].Max ||
                angles >= (Constants.CIRCLE_ANGLES - aspectsOrb[AspectEnum.Square].Max) && angles <= (Constants.CIRCLE_ANGLES - aspectsOrb[AspectEnum.Square].Min))
            {
                return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.Square);
            }
            else if (angles >= aspectsOrb[AspectEnum.Trine].Min && angles <= aspectsOrb[AspectEnum.Trine].Max ||
                angles >= (Constants.CIRCLE_ANGLES - aspectsOrb[AspectEnum.Trine].Max) && angles <= (Constants.CIRCLE_ANGLES - aspectsOrb[AspectEnum.Trine].Min))
            {
                return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.Trine);
            }
            else if (angles >= aspectsOrb[AspectEnum.Opposition].Min && angles <= aspectsOrb[AspectEnum.Opposition].Max ||
                angles >= (Constants.CIRCLE_ANGLES - aspectsOrb[AspectEnum.Opposition].Max) && angles <= (Constants.CIRCLE_ANGLES - aspectsOrb[AspectEnum.Opposition].Min))
            {
                return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.Opposition);
            }

            return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.None);
        }

        public List<AspectInfo> GetTransitMoonAspects(ChartInfo natalChart, DateTime startUtcDate, DateTime endUtcDate)
        {
            if (startUtcDate >= endUtcDate)
            {
                return new List<AspectInfo>();
            }

            //calculate moon info
            var moonTransitDict = new Dictionary<DateTime, PlanetInfo>();

            while (startUtcDate < endUtcDate)
            {
                var moonTransit = GetDayInfo(PlanetEnum.Moon, startUtcDate, out var error);
                moonTransitDict.Add(startUtcDate, moonTransit);

                startUtcDate = startUtcDate.AddHours(1);
            }

            //calculate aspects
            var aspects = new List<AspectInfo>();

            foreach (var planetEnum in Enum.GetValues(typeof(PlanetEnum)).Cast<PlanetEnum>())
            {
                var natalPlanetInfo = natalChart.Planets[planetEnum];

                var moonTransitInfo = new PlanetInfo(PlanetEnum.Moon, 0);

                var foundedAspect = AspectEnum.None;
                var startTime = DateTime.UtcNow;
                var endTime = DateTime.UtcNow;

                foreach (var moonTransit in moonTransitDict)
                {
                    var currentAspectInfo = GetAspect(natalPlanetInfo, moonTransit.Value, ChartTypeEnum.Transit);

                    if (currentAspectInfo.Aspect != AspectEnum.None && foundedAspect == AspectEnum.None)
                    {
                        startTime = moonTransit.Key;
                        moonTransitInfo = moonTransit.Value;
                        foundedAspect = currentAspectInfo.Aspect;
                    }
                    else if (currentAspectInfo.Aspect == AspectEnum.None && foundedAspect != AspectEnum.None)
                    {
                        endTime = moonTransit.Key;

                        var newAspect = new AspectInfo(natalPlanetInfo, moonTransitInfo, foundedAspect, startTime, endTime);
                        aspects.Add(newAspect);

                        foundedAspect = AspectEnum.None;
                    }
                }
            }

            return aspects;
        }










        //Нужен ли Dictionary?
        public Dictionary<DateTime, ChartInfo> GetData(DateTime startTime, DateTime endTime, TimeSpan interval, double logitude, double latitude)
        {
            if (startTime >= endTime)
            {
                return null; ;
            }

            var currentTime = startTime;

            var daysInfo = new Dictionary<DateTime, ChartInfo>();

            while (currentTime < endTime)
            {
                var dayInfo = GetDayInfo(currentTime, logitude, latitude, out var error);
                daysInfo.Add(currentTime, dayInfo);

                currentTime = currentTime.Add(interval);
            }

            _swissEphemeris.swe_close();

            return daysInfo;
        }

        //public Dictionary<DateTime, List<AspectInfo>> ProcessAspects0(ChartInfo birthDayInfo, List<ChartInfo> transitList)
        //{
        //    var dict = new Dictionary<DateTime, List<AspectInfo>>();

        //    //natal planet
        //    foreach (var natalPlanet in birthDayInfo.Planets.Values)
        //    {
        //        //transit date time
        //        foreach (var transit in transitList)
        //        {
        //            //transit planet
        //            foreach (var transitPlanet in transit.Planets.Values)
        //            {
        //                var aspect = GetAspect(natalPlanet, transitPlanet);

        //                if (aspect.Aspect == AspectEnum.None)
        //                {
        //                    continue;
        //                }

        //                if (dict.TryGetValue(transit.DateTime, out var aspectList))
        //                {
        //                    aspectList.Add(aspect);
        //                }
        //                else
        //                {
        //                    dict.Add(transit.DateTime, new List<AspectInfo>() { aspect });
        //                }
        //            }
        //        }
        //    }

        //    return dict;
        //}

        //public List<PlanetMain> ProcessAspects(ChartInfo birthDayInfo, List<ChartInfo> transitList)
        //{
        //    var planetMainList = new List<PlanetMain>();

        //    //natal planet
        //    foreach (var natalPlanet in birthDayInfo.Planets.Values)
        //    {
        //        var planetMain = new PlanetMain(natalPlanet.Planet);

        //        //transit date time
        //        foreach (var transit in transitList)
        //        {
        //            var aspects = new List<AspectInfo>();

        //            //transit planet
        //            foreach (var transitPlanet in transit.Planets.Values)
        //            {
        //                var aspect = GetAspect(natalPlanet, transitPlanet);

        //                if (aspect.Aspect == AspectEnum.None)
        //                {
        //                    continue;
        //                }

        //                aspects.Add(aspect);
        //            }

        //            planetMain.Aspects.Add(transit.DateTime, aspects);
        //        }

        //        planetMainList.Add(planetMain);
        //    }

        //    return planetMainList;
        //}

        //public Dictionary<DateTime, List<AspectInfo>> ProcessAspects2(ChartInfo birthDayInfo, List<ChartInfo> transitList)
        //{
        //    var dict = new Dictionary<DateTime, List<AspectInfo>>();

        //    //natal planet
        //    foreach (var natalPlanet in birthDayInfo.Planets.Values)
        //    {
        //        //transit date time
        //        foreach (var transit in transitList)
        //        {
        //            //transit planet
        //            foreach (var transitPlanet in transit.Planets.Values)
        //            {
        //                var aspect = GetAspect(natalPlanet, transitPlanet);

        //                if (aspect.Aspect == AspectEnum.None)
        //                {
        //                    continue;
        //                }

        //                if (dict.TryGetValue(transit.DateTime, out var aspectList))
        //                {
        //                    aspectList.Add(aspect);
        //                }
        //                else
        //                {
        //                    dict.Add(transit.DateTime, new List<AspectInfo>() { aspect });
        //                }
        //            }
        //        }
        //    }

        //    return dict;
        //}

        public void FillEphemeris(DateTime startDate, DateTime endDate, TimeSpan interval, double logitude, double latitude)
        {
            var saveDbTimeSpan = new TimeSpan(1, 0, 0, 0);

            var startIntervalDate = startDate;
            var endIntervalDate = startIntervalDate.Add(saveDbTimeSpan);

            while (startIntervalDate < endDate)
            {
                var dict = GetData(startIntervalDate, endIntervalDate, interval, logitude, latitude);

                var ephList = new List<Ephemeris>();

                foreach (var dtInfo in dict)
                {
                    var ephDb = new Ephemeris()
                    {
                        Id = long.Parse($"{dtInfo.Key.ToUniversalTime().ToString("yyyyMMddHHmmss")}"),
                        DateTime = dtInfo.Key.ToUniversalTime(),

                        SunAngles = dtInfo.Value.Planets[PlanetEnum.Sun].AbsolutAngles,
                        MoonAngles = dtInfo.Value.Planets[PlanetEnum.Moon].AbsolutAngles,

                        MercuryAngles = dtInfo.Value.Planets[PlanetEnum.Mercury].AbsolutAngles,
                        VenusAngles = dtInfo.Value.Planets[PlanetEnum.Venus].AbsolutAngles,
                        MarsAngles = dtInfo.Value.Planets[PlanetEnum.Mars].AbsolutAngles,

                        JupiterAngles = dtInfo.Value.Planets[PlanetEnum.Jupiter].AbsolutAngles,
                        SaturnAngles = dtInfo.Value.Planets[PlanetEnum.Saturn].AbsolutAngles,

                        UranAngles = dtInfo.Value.Planets[PlanetEnum.Uran].AbsolutAngles,
                        NeptuneAngles = dtInfo.Value.Planets[PlanetEnum.Neptune].AbsolutAngles,
                        PlutoAngles = dtInfo.Value.Planets[PlanetEnum.Pluto].AbsolutAngles
                    };

                    ephList.Add(ephDb);
                }

                _ephemerisProvider.AddEphemerises(ephList);

                startIntervalDate = startIntervalDate.Add(saveDbTimeSpan);
                endIntervalDate = endIntervalDate.Add(saveDbTimeSpan);
            }


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
                    return -1;
            }
        }

        private ChartInfo GetDayInfo(DateTime dateTime, double logitude, double latitude, out string error)
        {
            error = string.Empty;
            var dayInfo = new ChartInfo(dateTime);

            // Преобразует дату и время в юлианскую дату
            double minutePart = (dateTime.Minute / 60.0);
            double secondPart = (dateTime.Second / 3600.0);
            double day = _swissEphemeris.swe_julday(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour + minutePart + secondPart, _swissCalendarType);

            //Fill planets info
            foreach (var planetEnum in Enum.GetValues(typeof(PlanetEnum)).Cast<PlanetEnum>())
            {
                var planetInfo = GetPlanetInfo(planetEnum, day, _swissPositionType, out error);
                dayInfo.Planets.Add(planetEnum, planetInfo);
            }

            double[] cusps = new double[13]; // Куспиды домов (от 1 до 12)
            double[] ascmc = new double[10]; // Asc, MC и другие точки
            char houseSystem = 'P'; // Система домов: 'P' = Placidus

            _swissEphemeris.swe_houses(day, latitude, logitude, houseSystem, cusps, ascmc);

            //Fill houses info
            foreach (var houseEnum in Enum.GetValues(typeof(HouseEnum)).Cast<HouseEnum>())
            {
                if (houseEnum == HouseEnum.None)
                {
                    continue;
                }

                var houseInfo = new PositionInfo(cusps[(int)houseEnum]);
                dayInfo.Houses.Add(houseEnum, houseInfo);
            }

            //Fill planets houses info
            FillPlanetsHousesInfo(dayInfo);

            return dayInfo;
        }

        public PlanetInfo GetDayInfo(PlanetEnum planetEnum, DateTime dateTime, out string error)
        {
            error = string.Empty;

            // Преобразует дату и время в юлианскую дату
            double minutePart = (dateTime.Minute / 60.0);
            double secondPart = (dateTime.Second / 3600.0);
            double day = _swissEphemeris.swe_julday(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour + minutePart + secondPart, _swissCalendarType);

            var planetInfo = GetPlanetInfo(planetEnum, day, _swissPositionType, out error);

            return planetInfo;
        }

        private PlanetInfo GetPlanetInfo(PlanetEnum planetEnum, double day, int iflag, out string error)
        {
            var info = new double[6];
            error = string.Empty;

            var result = _swissEphemeris.swe_calc_ut(day, ConvertPlanetToSwiss(planetEnum), iflag, info, ref error);

            if (result != SwissEph.OK)
            {
                //Console.WriteLine($"Ошибка при расчете: {error}");
            }

            var planetInfo = new PlanetInfo(planetEnum, info[0], info[3]);

            return planetInfo;
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
                var planetRuler = Constants.HOUSES_RULER_DICT[zodiacEnum];

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
                var planetRuler = Constants.HOUSES_RULER_DICT[unknownZodiac];
                chartInfo.Planets[planetRuler].RulerHouses.Add(prevHouse);
            }
        }
    }
}
