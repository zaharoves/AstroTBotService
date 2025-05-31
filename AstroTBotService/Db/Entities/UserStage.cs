namespace AstroTBotService.Db.Entities
{
    public class UserStage
    {
        public UserStage()
        {
        }

        public UserStage(long id, string stage)
        {
            Id = id;
            Stage = stage;
        }

        public long? Id { get; set; }
        public string? Stage { get; set; }
    }
}
