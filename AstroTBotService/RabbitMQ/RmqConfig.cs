namespace AstroTBotService.RMQ
{
    public class RmqConfig
    {
        public string UserInfoQueue { get; set; }
        public string DailyForecastQueue { get; set; }

        public string HostName { get; set; } 
        public int Port { get; set; } 
        public string UserName { get; set; } 
        public string Password { get; set; }
        public string VirtualHost { get; set; }
    }
}
