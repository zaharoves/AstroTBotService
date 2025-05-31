using ProtoBuf;

namespace AstroTBotService.RMQ
{
    [ProtoContract]
    public class UserInfoMessage
    {
        [ProtoMember(1)]
        public string? MessageId { get; set; }

        [ProtoMember(2)]
        public DateTime? DateTime { get; set; }

        [ProtoMember(3)]
        public TimeSpan? GmtOffset { get; set; }
    }
}
