using AstroTBotService.Enums;

namespace AstroTBotService.AstroCalculation.Entities
{
    public class PlanetInfo : PositionInfo
    {
        public PlanetEnum Planet { get; }

        public double Speed { get; }

        public bool IsRetro => Speed < 0;

        public HouseEnum StandHouse { get; set; }

        public List<HouseEnum> RulerHouses { get; set; } = new List<HouseEnum>();

        public PlanetInfo(PlanetEnum planetEnum, double absolutAngles) 
            : base(absolutAngles)
        {
            Planet = planetEnum;
        }

        public PlanetInfo(PlanetEnum planetEnum, double absolutAngles, double speed) 
            : base(absolutAngles)
        {
            Planet = planetEnum;
            Speed = speed;
        }
    }
}
