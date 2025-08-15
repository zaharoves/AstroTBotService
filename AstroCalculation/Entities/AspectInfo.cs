using AstroCalculation.Enums;

namespace AstroCalculation.Entities
{
    public class AspectInfo
    {
        public PlanetInfo NatalPlanet { get; set; }

        public PlanetInfo TransitPlanet { get; set; }

        public AspectEnum Aspect { get; set; }

        public DateTime StartDate{ get; set; }

        public DateTime EndDate { get; set; }

        public AspectInfo(PlanetInfo natalPlanet, PlanetInfo transitPlanet, AspectEnum aspect)
        {
            NatalPlanet = natalPlanet;
            TransitPlanet = transitPlanet;
            Aspect = aspect;
        }

        public AspectInfo(PlanetInfo natalPlanet, PlanetInfo transitPlanet, AspectEnum aspect, DateTime startDate, DateTime endDate)
        {
            NatalPlanet = natalPlanet;
            TransitPlanet = transitPlanet;
            Aspect = aspect;

            StartDate = startDate;
            EndDate = endDate;
        }
    }
}
