using AstroTBotService.Entities;
using System.Threading.Tasks;

namespace AstroTBotService.Redis
{
    public interface IRedisService
    {
        Task SetMessageForbidTime(long mainUserId);
        Task<bool> CheckMessageForbidTime(long mainUserId);
        Task<bool> DeleteMessageForbidTime(long mainUserId);

        Task Set(string key, string value);
        Task<string> Get(string key);

        Task<bool> CreatePersonData(long mainUserId);
        Task<bool> CreatePersonDataForEdit(long mainUserId, long personId);


        Task<bool> CreateUserData(long mainUserId);

        Task<bool> SetPersonData(long mainUserId, RedisPersonData personData);


        Task<bool> SetPersonName(long mainUserId, string personName);
        Task<bool> SetPersonYearInterval(long mainUserId, int startYear);
        Task<bool> SetPersonYear(long mainUserId, int year);
        Task<bool> SetPersonMonth(long mainUserId, int month);
        Task<bool> SetPersonDay(long mainUserId, int day);

        Task<bool> SetPersonHour(long mainUserId, int hour);
        Task<bool> SetPersonMinute(long mainUserId, int minute);

        Task<bool> SetPersonTimeZone(long mainUserId, TimeSpan timeSpan);

        Task<bool> SetPersonLongitude(long mainUserId, double longitude);
        Task<bool> SetPersonLatitude(long mainUserId, double latitude);

        Task<bool> SetEditingPersonId(long mainUserId, long personId);
        Task<bool> SetEditingUserId(long mainUserId);


        Task<RedisPersonData> GetPersonData(long mainUserId);

        Task<bool> DeletePersonData(long mainUserId);
    }
}
