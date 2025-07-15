using AstroTBotService.Enums;

namespace AstroTBotService.AstroCalculation.Entities
{
    public class PlanetMain
    {
        public PlanetEnum Planet { get; }

        public Dictionary<DateTime, List<AspectInfo>> Aspects = new Dictionary<DateTime, List<AspectInfo>>();

        public PlanetMain(PlanetEnum planet) 
        {
            Planet = planet;
        }
    }
}
