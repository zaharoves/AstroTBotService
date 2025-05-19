namespace AstroTBotService.TBot
{
    public interface ITBotClientHelper
    {
        public Task SendMessageAsync(string rmqMessageId, string messageText);
    }
}
