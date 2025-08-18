using AstroTBotService.Configurations;
using AstroTBotService.Entities;
using AstroTBotService.TBot;
using Microsoft.Extensions.Options;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;

namespace AstroTBotService.Redis
{
    public class RedisService : IRedisService
    {
        private readonly IOptions<RedisConfig> _redisConfig;
        private readonly IDatabase _database;
        private readonly ILogger<TBotUpdateHandler> _logger;

        public RedisService(
            IOptions<RedisConfig> redisConfig,
            ILogger<TBotUpdateHandler> logger,
            IDatabase database)
        {
            _redisConfig = redisConfig;
            _logger = logger;
            _database = database;
        }

        public async Task SetMessageForbidTime(long mainUserId)
        {
            TimeSpan lifeTime = TimeSpan.FromMilliseconds(_redisConfig.Value.ForbidTimeMessageMillisec);

            var key = $"{mainUserId}{Constants.Redis.SEPARATOR}{Constants.Redis.FORBID_TIME}";

            bool isSuccess = await _database.StringSetAsync(key, string.Empty, lifeTime);

            if (!isSuccess)
            {
                _logger.LogError($"Set redis value error. UserId: {mainUserId}. Key: {key}. Value: \"\". Lifetime: {lifeTime}.");
            }
        }

        public async Task<bool> CheckMessageForbidTime(long mainUserId)
        {
            var key = $"{mainUserId}{Constants.Redis.SEPARATOR}{Constants.Redis.FORBID_TIME}";

            var value = await _database.StringGetAsync(key);

            return !value.IsNull;
        }

        public async Task<bool> DeleteMessageForbidTime(long mainUserId)
        {
            var key = $"{mainUserId}{Constants.Redis.SEPARATOR}{Constants.Redis.FORBID_TIME}";

            var isKeyExists = await _database.KeyExistsAsync(key);
            
            if (!isKeyExists)
            {
                return true;
            }

            var isSuccess = await _database.KeyDeleteAsync(key);

            if (!isSuccess)
            {
                _logger.LogError($"Can't delete redis value. Key: {key}.");
            }

            return isSuccess;
        }


        public async Task Set(string key, string value)
        {
            await _database.StringSetAsync(key, value);
        }

        public async Task<string> Get(string key)
        {
            var redisValue = await _database.StringGetAsync(key);

            if (!redisValue.HasValue)
            {
                _logger.LogError($"Can't find redis value. Key: {key}.");
                return string.Empty;
            }

            return redisValue.ToString();
        }

        public async Task<bool> CreatePersonData(long mainUserId)
        {
            var personData = new RedisPersonData()
            {
                IsUser = false
            };

            var json = _database.JSON();

            var isSuccess = await json.SetAsync(mainUserId.ToString(), ".", personData);

            if (!isSuccess)
            {
                _logger.LogError($"Can't set redis value. Key: {mainUserId}.");
            }

            return isSuccess;
        }

        public async Task<bool> CreatePersonDataForEdit(long mainUserId, long personId)
        {
            var personData = new RedisPersonData()
            {
                IsUser = false,
                EditingPersonType = Constants.UI.Buttons.PersonTypes.PERSON,
                EditingPersonId = personId
            };

            var json = _database.JSON();

            var isSuccess = await json.SetAsync(mainUserId.ToString(), ".", personData);

            if (!isSuccess)
            {
                _logger.LogError($"Can't set redis value. Key: {mainUserId}.");
            }

            return isSuccess;
        }

        public async Task<bool> CreateUserData(long mainUserId)
        {
            var personData = new RedisPersonData()
            {
                IsUser = true
            };

            var json = _database.JSON();

            var isSuccess = await json.SetAsync(mainUserId.ToString(), ".", personData);

            if (!isSuccess)
            {
                _logger.LogError($"Can't set redis value. Key: {mainUserId}.");
            }

            return isSuccess;
        }

        public async Task<bool> SetPersonData(long mainUserId, RedisPersonData personData)
        {
            var json = _database.JSON();

            var isSuccess = await json.SetAsync(mainUserId.ToString(), ".", personData);

            if (!isSuccess)
            {
                _logger.LogError($"Can't set redis value. Key: {mainUserId}.");
            }

            return isSuccess;
        }

        public async Task<bool> SetPersonName(long mainUserId, string personName)
        {
            if (string.IsNullOrWhiteSpace(personName))
            {
                return false;
            }

            personName = $"\"{personName}\"";

            await _database.ExecuteAsync(
                "JSON.SET",
                mainUserId,
                $"$.{nameof(RedisPersonData.Name)}",
                personName);

            return true;
        }

        public async Task<bool> SetPersonYearInterval(long mainUserId, int startYear)
        {
            await _database.ExecuteAsync(
                "JSON.SET",
                mainUserId,
                $"$.{nameof(RedisPersonData.StartYearInterval)}",
                startYear);

            return true;
        }

        public async Task<bool> SetPersonYear(long mainUserId, int year)
        {
            await _database.ExecuteAsync(
                "JSON.SET",
                mainUserId,
                $"$.{nameof(RedisPersonData.Year)}",
                year);

            return true;
        }

        public async Task<bool> SetPersonMonth(long mainUserId, int month)
        {
            await _database.ExecuteAsync(
                "JSON.SET",
                mainUserId,
                $"$.{nameof(RedisPersonData.Month)}",
                month);

            return true;
        }

        public async Task<bool> SetPersonDay(long mainUserId, int day)
        {
            await _database.ExecuteAsync(
                "JSON.SET",
                mainUserId,
                $"$.{nameof(RedisPersonData.Day)}",
                day);

            return true;
        }

        public async Task<bool> SetPersonHour(long mainUserId, int hour)
        {
            await _database.ExecuteAsync(
                "JSON.SET",
                mainUserId,
                $"$.{nameof(RedisPersonData.Hour)}",
                hour);

            return true;
        }

        public async Task<bool> SetPersonMinute(long mainUserId, int minute)
        {
            await _database.ExecuteAsync(
                "JSON.SET",
                mainUserId,
                $"$.{nameof(RedisPersonData.Minute)}",
                minute);

            return true;
        }

        public async Task<bool> SetPersonLongitude(long mainUserId, double longitude)
        {
            await _database.ExecuteAsync(
                "JSON.SET",
                mainUserId,
                $"$.{nameof(RedisPersonData.Longitude)}",
                longitude);

            return true;
        }

        public async Task<bool> SetPersonLatitude(long mainUserId, double latitude)
        {
            await _database.ExecuteAsync(
                "JSON.SET",
                mainUserId,
                $"$.{nameof(RedisPersonData.Latitude)}",
                latitude);

            return true;
        }

        public async Task<bool> SetPersonTimeZone(long mainUserId, TimeZoneInfo timeZone)
        {
            await _database.ExecuteAsync(
                "JSON.SET",
                mainUserId,
                $"$.{nameof(RedisPersonData.TimeZone)}",
                timeZone.ToString());

            return true;
        }


        public async Task<string> SetPersonDateTimeOffsetStr(long mainUserId, DateTimeOffset dateTimeOffset)
        {
            var dateTimeOffsetStr = dateTimeOffset.ToString("O");

            var redisDateTimeOffsetStr = $"\"{dateTimeOffsetStr}\"";  

            await _database.ExecuteAsync(
                "JSON.SET",
                mainUserId,
                $"$.{nameof(RedisPersonData.DateTimeOffsetString)}",
                redisDateTimeOffsetStr);

            return dateTimeOffsetStr;
        }

        public async Task<bool> SetEditingPersonId(long mainUserId, long personId)
        {
            // TODO MSET
            await _database.ExecuteAsync(
                "JSON.SET",
                mainUserId,
                $"$.{nameof(RedisPersonData.EditingPersonId)}",
                personId);

            var personType = $"\"{Constants.UI.Buttons.PersonTypes.PERSON}\"";

            await _database.ExecuteAsync(
                "JSON.SET",
                mainUserId,
                $"$.{nameof(RedisPersonData.EditingPersonType)}",
                personType);

            return true;
        }

        public async Task<bool> SetEditingUserId(long mainUserId)
        {
            // TODO MSET
            await _database.ExecuteAsync(
                "JSON.SET",
                mainUserId,
                $"$.{nameof(RedisPersonData.EditingPersonId)}",
                mainUserId);

            var personType = $"\"{Constants.UI.Buttons.PersonTypes.USER}\"";

            await _database.ExecuteAsync(
                "JSON.SET",
                mainUserId,
                $"$.{nameof(RedisPersonData.EditingPersonType)}",
                personType);

            return true;
        }

        public async Task<RedisPersonData> GetPersonData(long mainUserId)
        {
            var json = _database.JSON();

            if (!await _database.KeyExistsAsync(mainUserId.ToString()))
            {
                return new RedisPersonData();
            }

            return await json.GetAsync<RedisPersonData>(mainUserId.ToString());
        }

        public async Task<bool> DeletePersonData(long mainUserId)
        {
            var key = mainUserId.ToString();
            
            var keyExists = await _database.KeyExistsAsync(key);
            
            if (!keyExists)
            {
                return true;
            }

            var isSuccess = await _database.KeyDeleteAsync(key);

            if (!isSuccess)
            {
                _logger.LogError($"Can't delete redis value. Key: {key}.");
            }

            return isSuccess;
        }
    }
}
