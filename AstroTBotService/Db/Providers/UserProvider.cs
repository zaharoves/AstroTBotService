using AstroHandlerService.Db.Entities;
using AstroTBotService.Db.Entities;
using Microsoft.EntityFrameworkCore;

namespace AstroHandlerService.Db.Providers
{
    public class UserProvider : IUserProvider
    {
        private ApplicationContext _appContext { get; set; }

        public UserProvider(
            ApplicationContext appContext)
        {
            _appContext = appContext;
        }

        public async Task<AstroUser?> GetUser(long userId)
        {
            return await _appContext.AstroUsers
                .FirstOrDefaultAsync(x => x.Id == userId);
        }

        public async Task AddUser(AstroUser user)
        {
            if (user == null)
            {
                return;
            }

            _appContext.AstroUsers.Add(user);
            await _appContext.SaveChangesAsync();
        }

        public async Task RemoveUser(AstroUser user)
        {
            if (user == null)
            {
                return;
            }

            _appContext.AstroUsers.Remove(user);
            await _appContext.SaveChangesAsync();
        }

        public async Task EditUser(long userId, AstroUser editInfo)
        {
            if (editInfo == null)
            {
                return;
            }

            var user = await _appContext.AstroUsers
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return;
            }

            user.BirthDate = editInfo.BirthDate;
            user.GmtOffset = editInfo.GmtOffset;
            user.Language = editInfo.Language;

            
            _appContext.AstroUsers.Update(user);
            try
            {
                await _appContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {//TODO
                var a = 0;
            }
        }

        public async Task SetUserStage(long userId, string stageString)
        {
            var stage = _appContext.UsersStages
                .FirstOrDefaultAsync(x => x.Id == userId).Result;

            if (stage == null)
            {
                var userStage = new UserStage(userId, stageString);
                _appContext.UsersStages.Add(userStage);
            }
            else
            {
                stage.Stage = stageString;
                _appContext.UsersStages.Update(stage);
            }

            await _appContext.SaveChangesAsync();
        }

        public async Task<UserStage?> GetUserStage(long userId)
        {
            return await _appContext.UsersStages
                .FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}
