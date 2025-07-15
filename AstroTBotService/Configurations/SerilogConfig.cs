namespace AstroTBotService.Configurations
{
    public class SerilogConfig
    {
        public const string ConfigKey = "Serilog";

        public string ConnectionString { get; set; } = string.Empty;
        public string TableName { get; set; } = "Logs";
        public string DefaultMinimumLevel { get; set; } = "Information";

        public Dictionary<string, string> MinimumLevelOverride { get; set; } = new Dictionary<string, string>();
    }
}
