using AstroCalculation.Enums;

namespace AstroCalculation.Constants
{
    public static class Astro
    {
        public static ushort ZODIAC_ZERO = 0;
        public static ushort ZODIAC_ANGLES = 30;
        public static ushort CIRCLE_ANGLES = 360;

        public static class Aspects
        {
            public static ushort CONJUNCTION = 0;
            public static ushort SEXTILE = 60;
            public static ushort SQUARE = 90;
            public static ushort TRINE = 120;
            public static ushort OPPOSITION = 180;
        }

        public static readonly IReadOnlyDictionary<ZodiacEnum, PlanetEnum> HOUSES_RULER_DICT =
            new Dictionary<ZodiacEnum, PlanetEnum>()
        {
            { ZodiacEnum.Aries, PlanetEnum.Mars },
            { ZodiacEnum.Taurus, PlanetEnum.Venus },
            { ZodiacEnum.Gemini, PlanetEnum.Mercury },

            { ZodiacEnum.Cancer, PlanetEnum.Moon },
            { ZodiacEnum.Leo, PlanetEnum.Sun },
            { ZodiacEnum.Virgo, PlanetEnum.Mercury },

            { ZodiacEnum.Libra, PlanetEnum.Venus },
            { ZodiacEnum.Scorpio, PlanetEnum.Pluto },
            { ZodiacEnum.Sagittarius, PlanetEnum.Jupiter },

            { ZodiacEnum.Capricorn, PlanetEnum.Saturn },
            { ZodiacEnum.Aquarius, PlanetEnum.Uran },
            { ZodiacEnum.Pisces, PlanetEnum.Neptune }
        };
    }
}
