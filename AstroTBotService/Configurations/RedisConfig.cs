namespace AstroTBotService.Configurations
{
    public class RedisConfig
    {
        public const string ConfigKey = "Redis";

        public string ConnectionString { get; set; }
        public long ForbidTimeMessageMillisec { get; set; } = 5000;
    }
}
