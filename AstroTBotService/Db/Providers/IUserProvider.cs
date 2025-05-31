using AstroHandlerService.Db.Entities;
using AstroTBotService.Db.Entities;

namespace AstroHandlerService.Db.Providers
{
    public interface IUserProvider
    {
        Task<User?> GetUser(long userId);

        Task AddUser(User user);

        Task RemoveUser(User user);

        Task EditUser(long userId, User editInfo);

        Task SetUserStage(long userId, string stageString);

        Task<UserStage?> GetUserStage(long userId);
    }
}
