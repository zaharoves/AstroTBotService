using AstroTBotService.Enums;
using System.Globalization;
using static AstroTBotService.Constants.UI;

namespace AstroTBotService
{
    public static class Constants
    {
        public const int START_INTERVAL_YEAR = 1900;
        public const int YEARS_INTERVAL = 20;
        public const int YEARS_PER_ROW = 5;

        public static ushort ZODIAC_ZERO = 0;
        public static ushort ZODIAC_ANGLES = 30;
        public static ushort CIRCLE_ANGLES = 360;

        //Aspects
        public static ushort CONJUNCTION = 0;
        public static ushort SEXTILE = 60;
        public static ushort SQUARE = 90;
        public static ushort TRINE = 120;
        public static ushort OPPOSITION = 180;


        /// <summary>
        /// Max telegram message length
        /// </summary>
        public const int MAX_T_MESSAGE_LENGTH = 4096;

        //TODO Concurrent
        public static readonly IReadOnlyList<TimeSpan> TIME_ZONE_DICT =
            new List<TimeSpan>()
            {
                {new TimeSpan(-11, 0, 0) },
                {new TimeSpan(-10, 0, 0) },
                {new TimeSpan(-9, -30, 0) },
                {new TimeSpan(-9, 0, 0) },
                {new TimeSpan(-8, 0, 0) },
                {new TimeSpan(-7, 0, 0) },
                {new TimeSpan(-6, 0, 0) },
                {new TimeSpan(-5, 0, 0) },
                {new TimeSpan(-4, -30, 0) },
                {new TimeSpan(-4, 0, 0) },
                {new TimeSpan(-3, -30, 0) },
                {new TimeSpan(-3, 0, 0) },
                {new TimeSpan(-2, 0, 0) },
                {new TimeSpan(-1, 0, 0) },
                {new TimeSpan(0, 0, 0) },
                {new TimeSpan(1, 0, 0) },
                {new TimeSpan(2, 0, 0) },
                {new TimeSpan(3, 0, 0) },
                {new TimeSpan(3, 30, 0) },
                {new TimeSpan(4, 0, 0) },
                {new TimeSpan(4, 30, 0) },
                {new TimeSpan(5, 0, 0) },
                {new TimeSpan(5, 30, 0) },
                {new TimeSpan(6, 0, 0) },
                {new TimeSpan(6, 30, 0) },
                {new TimeSpan(7, 0, 0) },
                {new TimeSpan(8, 0, 0) },
                {new TimeSpan(9, 0, 0) },
                {new TimeSpan(9, 30, 0) },
                {new TimeSpan(10, 0, 0) },
                {new TimeSpan(11, 0, 0) },
                {new TimeSpan(12, 0, 0) }
        };

        //TODO Concurrent
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

        public static class UI
        {
            public static class MessageCommands
            {
                public const string START = "/start";
                public const string CHANGE_LANGUAGE = "/changelanguage";
                public const string SET_BIRTHDATE = "/setbirthdate";
                public const string SET_LOCATION = "/setbirthlocation";
            }

            public static class ButtonCommands
            {
                public const string IGNORE = "ignore";

                public const string DATE_PICKER = "dateTimePicker";

                public const string SET_BIRTHDATE = "setBirthdate";
                public const string SAVE_BIRTHDAY = "saveBirthday";
                public const string CHANGE_BIRTHDAY = "changeBirthday";

                public const string SET_BIRTH_LOCATION = "setBirthLocation";
                public const string SAVE_BIRTH_LOCATION = "saveBirthLocation";
                public const string CHANGE_BIRTH_LOCATION = "changeBirthLocation";

                public const string SEND_MAIN_MENU = "sendMainMenu";
                public const string EDIT_TO_MAIN_MENU = "editToMainMenu";

                public const string NATAL_CHART = "natalChart";
                public const string TRANSIT_FORECAST = "transitForecast";
                public const string DIRECTION_FORECAST = "directionForecast";

                public const string POSITIVE_FORECAST = "positiveForecast";

                public const string CHANGE_LANGUAGE = "changeLanguage";
            }

            //TODO COncurrent
            public static IReadOnlyDictionary<ChartTypeEnum, string> ChartTypeIconDict =
                new Dictionary<ChartTypeEnum, string>
                {
                    { ChartTypeEnum.Natal, Icons.Common.WHITE_CIRCLE },
                    { ChartTypeEnum.Transit, Icons.Common.BLUE_CIRCLE },
                    { ChartTypeEnum.Direction, Icons.Common.PURPLE_CIRCLE }
                };

            //TODO COncurrent
            public static IReadOnlyDictionary<ZodiacEnum, string> ZodiacIconDict =
                new Dictionary<ZodiacEnum, string>
                {
                    { ZodiacEnum.Aries, Icons.Zodiac.ARIES },
                    { ZodiacEnum.Taurus, Icons.Zodiac.TAURUS },
                    { ZodiacEnum.Gemini, Icons.Zodiac.GEMINI },

                    { ZodiacEnum.Cancer, Icons.Zodiac.CANCER },
                    { ZodiacEnum.Leo, Icons.Zodiac.LEO },
                    { ZodiacEnum.Virgo, Icons.Zodiac.VIRGO },

                    { ZodiacEnum.Libra, Icons.Zodiac.LIBRA },
                    { ZodiacEnum.Scorpio, Icons.Zodiac.SCORPIO },
                    { ZodiacEnum.Sagittarius, Icons.Zodiac.SAGITTARIUS },

                    { ZodiacEnum.Capricorn, Icons.Zodiac.CAPRICORN },
                    { ZodiacEnum.Aquarius, Icons.Zodiac.AQUARIUS },
                    { ZodiacEnum.Pisces, Icons.Zodiac.PISCES }
                };

            //TODO COncurrent
            public static IReadOnlyDictionary<PlanetEnum, string> PlanetIconDict =
                new Dictionary<PlanetEnum, string>
                {
                { PlanetEnum.Sun, Icons.Planets.SUN },
                { PlanetEnum.Moon, Icons.Planets.MOON },

                { PlanetEnum.Mercury, Icons.Planets.MERCURY },
                { PlanetEnum.Venus, Icons.Planets.VENUS },
                { PlanetEnum.Mars, Icons.Planets.MARS },

                { PlanetEnum.Jupiter, Icons.Planets.JUPPITER },
                { PlanetEnum.Saturn, Icons.Planets.SATURN },

                { PlanetEnum.Uran, Icons.Planets.URAN },
                { PlanetEnum.Neptune, Icons.Planets.NEPTUNE },
                { PlanetEnum.Pluto, Icons.Planets.PLUTO }
                };

            //TODO COncurrent
            public static IReadOnlyDictionary<AspectEnum, string> AspectIconDict =
                new Dictionary<AspectEnum, string>
                {
                { AspectEnum.None, string.Empty },
                { AspectEnum.Conjunction, Icons.Aspects.CONJUCTION },
                { AspectEnum.Sextile, Icons.Aspects.SEXTILE },
                { AspectEnum.Square, Icons.Aspects.SQUARE },
                { AspectEnum.Trine, Icons.Aspects.TRINE },
                { AspectEnum.Opposition, Icons.Aspects.OPPOSITION }
                };

            //TODO COncurrent
            public static IReadOnlyDictionary<string, CultureInfo> LocaleDict =
                new Dictionary<string, CultureInfo>
                {
                { "ru", new CultureInfo("ru-RU") },
                { "en", new CultureInfo("en-US") }
                };

            //TODO COncurrent
            public static IReadOnlyDictionary<string, (string Icon, string Description)> FlagsInfoDict =
                new Dictionary<string, (string Icon, string Description)>
                {
                { "ru-RU", (Icons.Flags.RUSSIAN, "Русский") },
                { "en-US", (Icons.Flags.ENGLISH, "English" ) }
                };

            public static class Icons
            {
                public static class Zodiac
                {
                    public const string ARIES = "♈";
                    public const string TAURUS = "♉";
                    public const string GEMINI = "♊";

                    public const string CANCER = "♋";
                    public const string LEO = "♌";
                    public const string VIRGO = "♍";

                    public const string LIBRA = "♎";
                    public const string SCORPIO = "♏";
                    public const string SAGITTARIUS = "♐";

                    public const string CAPRICORN = "♑";
                    public const string AQUARIUS = "♒";
                    public const string PISCES = "♓";
                }

                public static class Planets
                {
                    public const string SUN = "☉";
                    public const string MOON = "☽";

                    public const string MERCURY = "☿";
                    public const string VENUS = "♀";
                    public const string MARS = "♂";

                    public const string JUPPITER = "♃";
                    public const string SATURN = "♄";

                    public const string URAN = "♅";
                    public const string NEPTUNE = "♆";
                    public const string PLUTO = "♇";
                }

                public static class Aspects
                {
                    /// <summary>
                    /// 0 Angles
                    /// </summary>
                    public const string CONJUCTION = "☌";

                    /// <summary>
                    /// 60 Angles
                    /// </summary>
                    public const string SEXTILE = "⚹";

                    /// <summary>
                    /// 90 Angles
                    /// </summary>
                    public const string SQUARE = "☐";

                    /// <summary>
                    /// 120 Angles
                    /// </summary>
                    public const string TRINE = "△";

                    /// <summary>
                    /// 180 Angles
                    /// </summary>
                    public const string OPPOSITION = "☍";
                }

                public static class Common
                {
                    public const string EARTH = "🌍";
                    public const string MAP = "🗺️";

                    public const string RETRO = "Ⓡ";
                    public const string ANGLES = "°";
                    public const string MINUTES = "'";
                    public const string SECONDS = "\"";

                    public const string CHOOSED = "✅";
                    public const string REJECTED = "❌";
                    public const string EDIT = "✏️";
                    public const string SAVE = "💾";

                    public const string WARNING_RED = "❗";
                    public const string WARNING_WHITE = "❕";

                    public const string QUESTION_RED = "❓";
                    public const string QUESTION_WHITE = "❔";

                    public const string INFO = "💡";
                    public const string HOURGLASS = "⏳";

                    public const string NEXT = "➡️";
                    public const string PREVIOUS = "⬅️";

                    public const string SCIENCE = "⚛️";
                    public const string SUN = "☀️";

                    public const string PLUS = "➕";
                    public const string MINUS = "➖";

                    public const string RED_CIRCLE = "🔴";
                    public const string ORANGE_CIRCLE = "🟠";
                    public const string YELLOW_CIRCLE = "🟡";
                    public const string GREEN_CIRCLE = "🟢";
                    public const string BLUE_CIRCLE = "🔵";
                    public const string PURPLE_CIRCLE = "🟣";
                    public const string BROWN_CIRCLE = "🟤";
                    public const string BLACK_CIRCLE = "⚫";
                    public const string WHITE_CIRCLE = "⚪";

                    public const string RED_SQUARE = "🟥";
                    public const string ORANGE_SQUARE = "🟧";
                    public const string YELLOW_SQUARE = "🟨";
                    public const string GREEN_SQUARE = "🟩";
                    public const string BLUE_SQUARE = "🟦";
                    public const string PURPLE_SQUARE = "🟪";
                    public const string BROWN_SQUARE = "🟫";
                    public const string BLACK_SQUARE = "⬛";
                    public const string WHITE_SQUARE = "⬜";

                    public const string CLOCK_1 = "🕐";
                    public const string CLOCK_2 = "🕑";
                    public const string CLOCK_3 = "🕒";
                    public const string CLOCK_4 = "🕓";
                    public const string CLOCK_5 = "🕔";

                    public const string CLOCK_6 = "🕜";
                    public const string CLOCK_7 = "🕝";
                    public const string CLOCK_8 = "🕞";
                    public const string CLOCK_9 = "🕟";
                    public const string CLOCK_10 = "🕠";
                }

                public static class Flags
                {
                    public const string RUSSIAN = "🇷🇺";
                    public const string ENGLISH = "🇬🇧";
                }
            }
        }
    }
}
