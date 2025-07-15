using AstroTBotService.Enums;

namespace AstroTBotService.AstroCalculation.Entities
{
    public class PositionInfo : ICloneable
    {
        public PositionInfo(double absolutAngles)
        {
            absolutAngles = absolutAngles >= 360 || absolutAngles < 0 ? 0 : absolutAngles;

            AbsolutAngles = absolutAngles;

            int zodiacInt = (int)absolutAngles / Constants.ZODIAC_ANGLES;
            Zodiac = (ZodiacEnum)Enum.GetValues(typeof(ZodiacEnum)).GetValue(zodiacInt);

            var zodiacAbsAngles = absolutAngles % Constants.ZODIAC_ANGLES;

            ZodiacAngles = (int)zodiacAbsAngles;

            var mins = (zodiacAbsAngles - ZodiacAngles) * 60;

            ZodiacMinutes = (int)mins;
            ZodiacSeconds = (int)((mins - ZodiacMinutes) * 60);
        }

        ///Take angles 0 -> 360
        ///where zero - 0 angles of <see cref="ZodiacEnum.Aries"/>
        public double AbsolutAngles { get; }

        public ZodiacEnum Zodiac { get; }

        public int ZodiacAngles { get; }
        public int ZodiacMinutes { get; }
        public int ZodiacSeconds { get; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
