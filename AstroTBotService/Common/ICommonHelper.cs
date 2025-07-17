using AstroTBotService.AstroCalculation.Entities;
using AstroTBotService.Enums;

namespace AstroTBotService.Common
{
    public interface ICommonHelper
    {
        List<AspectInfo> GetNatalAspects(ChartInfo natalChart);

        List<AspectInfo> GetTransitAspects(ChartInfo natalChart, ChartInfo transitChart);

        List<AspectInfo> GetTransitAspects(ChartInfo natalChart, ChartInfo transitChart, params PlanetEnum[] dayPlanets);

        List<AspectInfo> GetTransitPlanetAspects(ChartInfo natalChart, Dictionary<DateTime, PlanetInfo> planetDaysInfo, PlanetEnum planetEnum);

        List<AspectInfo> GetDirectionAspects(ChartInfo natalChart, ChartInfo directionChart);
    }
}
