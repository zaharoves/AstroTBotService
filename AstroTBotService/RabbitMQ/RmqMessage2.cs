using ProtoBuf;

namespace AstroTBotService.RMQ
{
    [ProtoContract]
    public class RmqMessage2
    {
        [ProtoMember(1)]
        public string Id { get; set; }

        [ProtoMember(2)]
        public string MessageText { get; set; }
    }
}
