using AstroCalculation.Enums;

namespace AstroCalculation.Entities
{
    public class ChartInfo : ICloneable
    {
        public Dictionary<PlanetEnum, PlanetInfo> Planets { get; set; }
        public Dictionary<HouseEnum, PositionInfo> Houses { get; set; }
        public DateTime DateTime { get; set; }

        public ChartInfo(DateTime dateTime) : base()
        {
            DateTime = dateTime;

            Planets = new Dictionary<PlanetEnum, PlanetInfo>();
            Houses = new Dictionary<HouseEnum, PositionInfo>();
        }

        public object Clone()
        {
            var chart = new ChartInfo(DateTime);

            chart.Houses = Houses.ToDictionary(
                entry => entry.Key,
                entry => (PositionInfo)entry.Value.Clone());

            chart.Planets = Planets.ToDictionary(
                entry => entry.Key,
                entry => (PlanetInfo)entry.Value.Clone());

            return chart;
        }
    }
}
