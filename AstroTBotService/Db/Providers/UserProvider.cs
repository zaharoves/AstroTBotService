using AstroTBotService.Db.Entities;
using AstroTBotService.Enums;
using Microsoft.EntityFrameworkCore;

namespace AstroTBotService.Db.Providers
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

        public async Task EditBirthday(long userId, DateTime? birthDateTime, TimeSpan? gmtOffset)
        {
            var user = await _appContext.AstroUsers
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return;
            }

            if (birthDateTime.HasValue)
            {
                birthDateTime = DateTime.SpecifyKind(birthDateTime.Value, DateTimeKind.Utc);
            }

            user.BirthDate = birthDateTime;
            user.GmtOffset = gmtOffset;

            _appContext.AstroUsers.Update(user);

            try
            {
                await _appContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //TODO
                var a = 0;
            }
        }

        public async Task EditLanguage(long userId, string? language)
        {
            var user = await _appContext.AstroUsers
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(language))
            {
                return;
            }

            user.Language = language;

            _appContext.AstroUsers.Update(user);
            try
            {
                await _appContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //TODO
                var a = 0;
            }
        }

        public async Task EditLocation(long userId, double? longitude, double? latitude)
        {
            var user = await _appContext.AstroUsers
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return;
            }

            user.Longitude = longitude;
            user.Latitude = latitude;

            _appContext.AstroUsers.Update(user);

            try
            {
                await _appContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //TODO
                var a = 0;
            }
        }

        public async Task EditHouseSystem(long userId, HouseSystemEnum houseSystem)
        {
            var user = await _appContext.AstroUsers
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return;
            }

            user.HouseSystem = houseSystem;

            _appContext.AstroUsers.Update(user);
            try
            {
                await _appContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //TODO
                var a = 0;
            }
        }

        public async Task SetUserStage(long userId, ChatStageEnum? stageEnum)
        {
            var stage = _appContext.UsersStages
                .FirstOrDefaultAsync(x => x.Id == userId).Result;

            if (stage == null)
            {
                var userStage = new UserStage(userId, stageEnum?.ToString());
                _appContext.UsersStages.Add(userStage);
            }
            else
            {
                stage.Stage = stageEnum?.ToString();
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
