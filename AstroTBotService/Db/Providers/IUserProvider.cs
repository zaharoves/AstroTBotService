using AstroTBotService.Db.Entities;
using AstroTBotService.Enums;

namespace AstroTBotService.Db.Providers
{
    public interface IUserProvider
    {
        Task<AstroUser?> GetUser(long userId);

        Task AddUser(AstroUser user);

        Task RemoveUser(AstroUser user);

        Task EditBirthday(long userId, DateTime? birthDateTime, TimeSpan? gmtOffset);
        Task EditLanguage(long userId, string? language);
        Task EditLocation(long userId, double? longitude, double? latitude);
        Task EditHouseSystem(long userId, HouseSystemEnum houseSystem);


        Task SetUserStage(long userId, ChatStageEnum? stageString);

        Task<UserStage?> GetUserStage(long userId);
    }
}
