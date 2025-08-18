using AstroCalculation.Enums;

namespace AstroCalculation.Entities
{
    public class ChartInfo : ICloneable
    {
        public Dictionary<PlanetEnum, PlanetInfo> Planets { get; set; }
        public Dictionary<HouseEnum, PositionInfo> Houses { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; }

        public ChartInfo(DateTimeOffset dateTimeOffset) : base()
        {
            DateTimeOffset = dateTimeOffset;

            Planets = new Dictionary<PlanetEnum, PlanetInfo>();
            Houses = new Dictionary<HouseEnum, PositionInfo>();
        }

        public object Clone()
        {
            var chart = new ChartInfo(DateTimeOffset);

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
