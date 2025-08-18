using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace AstroTBotService.Db.Entities
{
    public interface IAstroPerson
    {
        long? Id { get; set; }

        DateTime? UtcBirthDate { get; set; }

        TimeSpan? TimeZoneOffset { get; set; }

        DateTimeOffset LocalDateTimeOffset { get; }

        double? Longitude { get; set; }
        double? Latitude { get; set; }


        long? ParentUserId { get; set; }

        AstroUser? ParentUser { get; set; }

        string? Name { get; set; }
        bool? IsChosen { get; set; }

        bool IsUser { get; }

        string DateToLongString(CultureInfo cultureInfo);
        string DateToShortString(CultureInfo cultureInfo);
    }
}
