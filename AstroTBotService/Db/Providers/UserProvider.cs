using AstroCalculation.Enums;
using AstroTBotService.Db.Entities;
using AstroTBotService.Entities;
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

        #region User

        public async Task<AstroUser?> GetUser(long userId)
        {
            return await _appContext.AstroUsers
                .Include(p => p.ChildPersons)
                .SingleOrDefaultAsync(p => p.Id == userId);
        }

        public async Task AddUser(AstroUser user)
        {
            if (user == null)
            {
                return;
            }

            user.IsChosen = true;

            _appContext.AstroUsers.Add(user);
            await _appContext.SaveChangesAsync();
        }

        public async Task EditUser(long userId, RedisPersonData personData)
        {
            var user = await _appContext.AstroUsers
                .FirstOrDefaultAsync(p => p.Id == userId);

            if (user == null)
            {
                return;
            }

            user.BirthDate = personData.GetDateTime();
            user.TimeZoneOffset = personData.GetTimeZone();
            user.Longitude = personData.Longitude;
            user.Latitude = personData.Latitude;

            _appContext.AstroUsers.Update(user);
            await _appContext.SaveChangesAsync();
        }

        public async Task DeleteUser(AstroUser user)
        {
            if (user == null)
            {
                return;
            }

            _appContext.AstroUsers.Remove(user);
            await _appContext.SaveChangesAsync();
        }

        public async Task<IList<AstroPerson>?> GetUsersPersons(long userId)
        {
            return await _appContext.AstroPersons
                .Where(p => p.ParentUserId == userId)
                .ToListAsync();
        }

        public async Task<UserStage?> GetUserStage(long userId)
        {
            return await _appContext.UsersStages
                .FirstOrDefaultAsync(p => p.Id == userId);
        }

        public async Task SetUserStage(long userId, ChatStageEnum stageEnum)
        {
            var stage = await _appContext.UsersStages
                .FirstOrDefaultAsync(p => p.Id == userId);

            if (stage == null)
            {
                var userStage = new UserStage(userId, stageEnum.ToString());
                _appContext.UsersStages.Add(userStage);
            }
            else
            {
                stage.Stage = stageEnum.ToString();
                _appContext.UsersStages.Update(stage);
            }

            await _appContext.SaveChangesAsync();
        }

        public async Task EditLanguage(long userId, string? language)
        {
            var user = await _appContext.AstroUsers
                .FirstOrDefaultAsync(p => p.Id == userId);

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

            await _appContext.SaveChangesAsync();
        }

        public async Task EditHouseSystem(long userId, HouseSystemEnum houseSystem)
        {
            var user = await _appContext.AstroUsers
                .FirstOrDefaultAsync(p => p.Id == userId);

            if (user == null)
            {
                return;
            }

            user.HouseSystem = houseSystem;

            _appContext.AstroUsers.Update(user);

            await _appContext.SaveChangesAsync();
        }

        public async Task UpdateUserName(long userId, string userName)
        {
            var user = await _appContext.AstroUsers
                .FirstOrDefaultAsync(p => p.Id == userId);

            if (user == null)
            {
                return;
            }

            user.Name = userName;

            _appContext.AstroUsers.Update(user);

            await _appContext.SaveChangesAsync();
        }

        public async Task<bool> IsNewUser(long userId)
        {
            var user = await _appContext.AstroUsers
                .SingleOrDefaultAsync(p => p.Id == userId);

            return user?.BirthDate == null;
        }

        #endregion

        #region Person

        public async Task<AstroPerson?> GetPerson(long personId)
        {
            return await _appContext.AstroPersons
                .Where(p => p.Id == personId)
                .FirstOrDefaultAsync();
        }

        public async Task AddPerson(long userId, RedisPersonData personData)
        {
            if (personData == null)
            {
                return;
            }

            var astroPerson = new AstroPerson()
            {
                Name = personData.Name,
                BirthDate = personData.GetDateTime(),
                TimeZoneOffset = personData.GetTimeZone(),
                ParentUserId = userId,
                Longitude = personData.Longitude,
                Latitude = personData.Latitude,
                IsChosen = false
            };

            _appContext.AstroPersons.Add(astroPerson);
            await _appContext.SaveChangesAsync();
        }

        public async Task AddOrEditPerson(long userId, long personId, RedisPersonData personData)
        {
            if (personData == null)
            {
                return;
            }

            var person = await _appContext.AstroPersons
                .FirstOrDefaultAsync(p => p.Id == personId);

            if (person == null)
            {
                await AddPerson(userId, personData);
                return;
            }

            person.Name = personData.Name;
            person.BirthDate = personData.GetDateTime();
            person.TimeZoneOffset = personData.GetTimeZone();
            person.ParentUserId = userId;
            person.Longitude = personData.Longitude;
            person.Latitude = personData.Latitude;

            _appContext.AstroPersons.Update(person);
            await _appContext.SaveChangesAsync();
        }

        public async Task DeletePerson(AstroPerson person)
        {
            if (person == null)
            {
                return;
            }

            _appContext.AstroPersons.Remove(person);
            await _appContext.SaveChangesAsync();
        }

        public async Task DeletePerson(long id)
        {
            await _appContext.AstroPersons
                .Where(p => p.Id == id)
                .ExecuteDeleteAsync();

            await _appContext.SaveChangesAsync();
        }

        public async Task UpdateChoosePersons(IAstroPerson firstPerson, IAstroPerson secondPerson)
        {
            if (firstPerson.IsUser == true)
            {
                _appContext.AstroUsers.Update((AstroUser)firstPerson);
            }
            else
            {
                _appContext.AstroPersons.Update((AstroPerson)firstPerson);
            }

            if (secondPerson.IsUser == true)
            {
                _appContext.AstroUsers.Update((AstroUser)secondPerson);
            }
            else
            {
                _appContext.AstroPersons.Update((AstroPerson)secondPerson);
            }

            await _appContext.SaveChangesAsync();
        }

        #endregion
    }
}