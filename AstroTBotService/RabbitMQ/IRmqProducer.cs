namespace AstroTBotService.RMQ
{
    public interface IRmqProducer
    {
        public void SendMessage<T>(string messageId, T message) where T : class;
    }
}
