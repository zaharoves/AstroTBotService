using AstroTBotService.Entities;
using ProtoBuf;

namespace AstroTBotService.RMQ
{
    [ProtoContract]
    public class RmqMessage
    {
        [ProtoMember(1)]
        public string MessageId { get; set; }

        [ProtoMember(2)]
        public DatePickerData DatePickerData { get; set; }
    }
}
