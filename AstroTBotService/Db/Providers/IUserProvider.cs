using AstroCalculation.Enums;
using AstroTBotService.Db.Entities;
using AstroTBotService.Entities;
using AstroTBotService.Enums;

namespace AstroTBotService.Db.Providers
{
    public interface IUserProvider
    {
        Task<AstroUser?> GetUser(long userId);
        Task AddUser(AstroUser user);
        Task EditUser(long userId, RedisPersonData personData);

        Task DeleteUser(AstroUser user);

        Task<IList<AstroPerson>?> GetUsersPersons(long userId);

        Task<UserStage?> GetUserStage(long userId);
        Task SetUserStage(long userId, ChatStageEnum stageString);

        Task EditLanguage(long userId, string? language);
        Task EditHouseSystem(long userId, HouseSystemEnum houseSystem);
        Task EditUserName(long userId, string userName);

        Task<bool> IsNewUser(long userId);


        Task<AstroPerson?> GetPerson(long personId);
        Task AddPerson(long userId, RedisPersonData personData);
        Task AddOrEditPerson(long userId, long personId, RedisPersonData personData);
        Task DeletePerson(AstroPerson person);


        Task UpdateChoosePersons(IAstroPerson firstPerson, IAstroPerson secondPerson);
    }
}
