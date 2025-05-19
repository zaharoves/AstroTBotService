namespace AstroTBotService.RMQ
{
    public class RmqConfig
    {
        public string QueueName1 { get; set; }
        public string QueueName2 { get; set; }

        public string HostName { get; set; } 
        public int Port { get; set; } 
        public string UserName { get; set; } 
        public string Password { get; set; }
        public string VirtualHost { get; set; }

        public static RmqConfig CreateDefault()
        {
            return new RmqConfig()
            {
                QueueName1 = "default_queue_name",
                QueueName2 = "default_queue_name2",

                HostName = "localhost",
                Port = 5672, 
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/"
            };
        }
    }
}
