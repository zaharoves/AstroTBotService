using AstroCalculation.Enums;

namespace AstroCalculation.Entities
{
    public class PlanetInfo : PositionInfo
    {
        public PlanetEnum Planet { get; }

        public double Speed { get; }

        /// <summary>
        /// Reverse movement of the planet
        /// </summary>
        public bool IsRetro => Speed < 0;

        /// <summary>
        /// Where the planet stands
        /// </summary>
        public HouseEnum StandHouse { get; set; }

        /// <summary>
        /// Houses, that are ruled by the planet
        /// </summary>
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
