using AstroHandlerService.Db.Entities;
using AstroTBotService.Db.Entities;

namespace AstroHandlerService.Db.Providers
{
    public interface IUserProvider
    {
        Task<AstroUser?> GetUser(long userId);

        Task AddUser(AstroUser user);

        Task RemoveUser(AstroUser user);

        Task EditUser(long userId, AstroUser editInfo);

        Task SetUserStage(long userId, string stageString);

        Task<UserStage?> GetUserStage(long userId);
    }
}
