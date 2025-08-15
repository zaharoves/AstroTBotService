using AstroCalculation.Configurations;
using AstroCalculation.Entities;
using AstroCalculation.Enums;
using AstroCalculation.Interfaces;
using Microsoft.Extensions.Options;

namespace AstroCalculation
{
    public class CommonHelper : ICommonHelper
    {
        private readonly Dictionary<PlanetEnum, AspectOrbDictionary> _natalOrbs;
        private readonly Dictionary<PlanetEnum, AspectOrbDictionary> _transitOrbs;
        private readonly Dictionary<PlanetEnum, AspectOrbDictionary> _directionOrbs;

        public CommonHelper(
            IOptions<AstroCalculationConfig> astroConfiguration)
        {
            _natalOrbs = new Dictionary<PlanetEnum, AspectOrbDictionary>()
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

            _transitOrbs = new Dictionary<PlanetEnum, AspectOrbDictionary>()
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

            _directionOrbs = new Dictionary<PlanetEnum, AspectOrbDictionary>()
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

        public List<AspectInfo> GetTransitPlanetAspects(ChartInfo natalChart, Dictionary<DateTime, PlanetInfo> planetInfoList, PlanetEnum planetEnum)
        {
            var aspects = new List<AspectInfo>();

            foreach (var natalPlanetEnum in Enum.GetValues(typeof(PlanetEnum)).Cast<PlanetEnum>())
            {
                var natalPlanetInfo = natalChart.Planets[natalPlanetEnum];

                var moonTransitInfo = new PlanetInfo(planetEnum, 0);

                var foundedAspect = AspectEnum.None;

                var startTime = DateTime.UtcNow;
                var endTime = DateTime.UtcNow;

                foreach (var moonDayInfo in planetInfoList)
                {
                    var currentAspectInfo = GetAspect(natalPlanetInfo, moonDayInfo.Value, ChartTypeEnum.Transit);

                    if (currentAspectInfo.Aspect != AspectEnum.None
                        && foundedAspect == AspectEnum.None)
                    {
                        startTime = moonDayInfo.Key;
                        moonTransitInfo = moonDayInfo.Value;
                        foundedAspect = currentAspectInfo.Aspect;
                    }
                    else if (currentAspectInfo.Aspect == AspectEnum.None
                        && foundedAspect != AspectEnum.None)
                    {
                        endTime = moonDayInfo.Key;

                        var newAspect = new AspectInfo(natalPlanetInfo, moonTransitInfo, foundedAspect, startTime, endTime);
                        aspects.Add(newAspect);

                        foundedAspect = AspectEnum.None;
                    }
                }
            }

            return aspects;
        }

        public List<AspectInfo> GetTransitAspects(ChartInfo natalChart, ChartInfo transitChart)
        {
            return GetAspects(natalChart, transitChart, ChartTypeEnum.Transit);

        }

        public List<AspectInfo> GetTransitAspects(ChartInfo natalChart, ChartInfo transitChart, params PlanetEnum[] dayPlanets)
        {
            return GetAspects(natalChart, transitChart, ChartTypeEnum.Transit, dayPlanets);

        }

        public List<AspectInfo> GetDirectionAspects(ChartInfo natalChart, ChartInfo directionChart)
        {
            return GetAspects(natalChart, directionChart, ChartTypeEnum.Direction);

        }

        private List<AspectInfo> GetAspects(ChartInfo natalChart, ChartInfo dayChart, ChartTypeEnum chartType)
        {
            if (chartType == ChartTypeEnum.Natal)
            {
                return GetNatalAspects(natalChart);
            }

            var aspects = new List<AspectInfo>();

            foreach (var natalPlanet in natalChart.Planets.Values)
            {
                foreach (var dayPlanet in dayChart.Planets.Values)
                {
                    var aspectInfo = GetAspect(natalPlanet, dayPlanet, chartType);

                    if (aspectInfo.Aspect != AspectEnum.None)
                    {
                        aspects.Add(aspectInfo);
                    }

                }
            }

            return aspects;
        }

        private List<AspectInfo> GetAspects(ChartInfo natalChart, ChartInfo dayChart, ChartTypeEnum chartType, params PlanetEnum[] dayPlanets)
        {
            if (chartType == ChartTypeEnum.Natal)
            {
                return GetNatalAspects(natalChart);
            }

            var aspects = new List<AspectInfo>();

            if (dayPlanets.Count() == 0)
            {
                return aspects;
            }

            foreach (var natalPlanet in natalChart.Planets.Values)
            {
                foreach (var dayPlanet in dayChart.Planets.Values)
                {
                    if (!dayPlanets.Contains(dayPlanet.Planet))
                    {
                        continue;
                    }

                    var aspectInfo = GetAspect(natalPlanet, dayPlanet, chartType);

                    if (aspectInfo.Aspect != AspectEnum.None)
                    {
                        aspects.Add(aspectInfo);
                    }
                }
            }

            return aspects;
        }

        private AspectInfo GetAspect(PlanetInfo natalPlanet, PlanetInfo transitPlanet, ChartTypeEnum chartType)
        {
            var ordDictionary = GetOrbDictionary(chartType);

            if (!ordDictionary.TryGetValue(natalPlanet.Planet, out var aspectsOrb))
            {
                return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.None);
            }

            var angles = Math.Abs(natalPlanet.AbsolutAngles - transitPlanet.AbsolutAngles);

            if (angles >= aspectsOrb[AspectEnum.Conjunction].Min && angles <= Constants.Astro.CIRCLE_ANGLES ||
               angles <= aspectsOrb[AspectEnum.Conjunction].Max && angles >= Constants.Astro.ZODIAC_ZERO)
            {
                return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.Conjunction);
            }
            else if (angles >= aspectsOrb[AspectEnum.Sextile].Min && angles <= aspectsOrb[AspectEnum.Sextile].Max ||
                angles >= (Constants.Astro.CIRCLE_ANGLES - aspectsOrb[AspectEnum.Sextile].Max) && angles <= (Constants.Astro.CIRCLE_ANGLES - aspectsOrb[AspectEnum.Sextile].Min))
            {
                return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.Sextile);
            }
            else if (angles >= aspectsOrb[AspectEnum.Square].Min && angles <= aspectsOrb[AspectEnum.Square].Max ||
                angles >= (Constants.Astro.CIRCLE_ANGLES - aspectsOrb[AspectEnum.Square].Max) && angles <= (Constants.Astro.CIRCLE_ANGLES - aspectsOrb[AspectEnum.Square].Min))
            {
                return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.Square);
            }
            else if (angles >= aspectsOrb[AspectEnum.Trine].Min && angles <= aspectsOrb[AspectEnum.Trine].Max ||
                angles >= (Constants.Astro.CIRCLE_ANGLES - aspectsOrb[AspectEnum.Trine].Max) && angles <= (Constants.Astro.CIRCLE_ANGLES - aspectsOrb[AspectEnum.Trine].Min))
            {
                return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.Trine);
            }
            else if (angles >= aspectsOrb[AspectEnum.Opposition].Min && angles <= aspectsOrb[AspectEnum.Opposition].Max ||
                angles >= (Constants.Astro.CIRCLE_ANGLES - aspectsOrb[AspectEnum.Opposition].Max) && angles <= (Constants.Astro.CIRCLE_ANGLES - aspectsOrb[AspectEnum.Opposition].Min))
            {
                return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.Opposition);
            }

            return new AspectInfo(natalPlanet, transitPlanet, AspectEnum.None);
        }

        private Dictionary<PlanetEnum, AspectOrbDictionary> GetOrbDictionary(ChartTypeEnum chartType)
        {
            switch (chartType)
            {
                case ChartTypeEnum.Natal:
                    return _natalOrbs;
                case ChartTypeEnum.Transit:
                    return _transitOrbs;
                case ChartTypeEnum.Direction:
                    return _directionOrbs;
            }

            return new Dictionary<PlanetEnum, AspectOrbDictionary>();
        }
    }
}
