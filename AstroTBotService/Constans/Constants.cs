using System.Collections.ObjectModel;

namespace AstroTBotService.Constans
{
    public static class Constants
    {
        public const string WELCOME_MESSAGE = "Приветственное сообщение!";

        public const string MAIN_MENU_MESSAGE = "Необходимо заполнить дату Вашего рождения.";
        public const string MAIN_MENU_MESSAGE_BIRTHDAY = "Дата вашего рождения: {0}\nМожно запустить процесс расчета.";

        public const int START_INTERVAL_YEAR = 1900;

        public static readonly ReadOnlyCollection<(int TimeZoneInt, string Description)> TIME_ZONE_DICT =
            new ReadOnlyCollection<(int, string)>(new List<(int, string)>()
        {
            { (-12, "Россия. Петропавловск-Камчатский")},
            { (-11, "")},
            { (-10, "Америка. шт.Гавайи")},
            { (-9, "")},
            { (-8, "Америка. шт.Аляска")},
            { (-7, "Америка. шт.Орегон")},
            { (-6, "Мексика. Мехико")},
            { (-5, "Америка. шт.Оклахома")},
            { (-4, "Америка. Вашингтон")},
            { (-3, "Аргентина. Буэнос-Айрэс")},
            { (-2, "")},
            { (-1, "Кабо-Верде")},
            { (0, "Исландия. Рейкьявик")},
            { (1, "Великобритания. Лондон")},
            { (2, "Германия. Берлин")},
            { (3, "Россия. Москва")},
            { (4, "Грузия. Тбилиси")},
            { (5, "Казахстан. Астана")},
            { (6, "Киргизия. Бишкек")},
            { (7, "Вьетнам. Ханой")},
            { (8, "Китай. Пекин")},
            { (9, "Япония. Токио")},
            { (10, "Россия. Хабаровск")},
            { (11, "Россия. Южно-Сахалинск")},
            { (12, "Россия. Петропавловск-Камчатский")}
        });

        public static class MessageCommands
        {
            public const string START = "/start";
        }

        public static class ButtonCommands
        {
            public const string IGNORE = "ignore";

            public const string DATE_PICKER = "date_time_picker";
            public const string SAVE_BIRTHDAY = "save_birthday";
            public const string CHANGE_BIRTHDAY = "change_birthday";
            public const string TO_MAIN_MENU = "to_main_menu";
            
            public const string SET_BIRTHDAY = "set_birthday";
            public const string TODAY_FORECAST = "today_forecast";
            public const string POSITIVE_FORECAST = "positive_forecast:";
        }


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
                public const string SQUARE = "◻";

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
                public const string CHOOSED = "✅";
                public const string REJECTED = "❌";
                public const string EDIT = "✏️";

                public const string WARNING_RED = "❗";
                public const string WARNING_WHITE = "❕";

                public const string QUESTION_RED = "❓";
                public const string QUESTION_WHITE = "❔";

                public const string INFO = "💡";

                public const string NEXT = "➡️";
                public const string PREVIOUS = "⬅️";
            }
        }   
    }
}
