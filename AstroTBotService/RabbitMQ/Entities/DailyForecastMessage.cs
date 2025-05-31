using ProtoBuf;

namespace AstroTBotService.RMQ
{
    [ProtoContract]
    public class DailyForecastMessage
    {
        [ProtoMember(1)]
        public string Id { get; set; }

        [ProtoMember(2)]
        public List<DailyForecastAspect> Aspects { get; set; }

        [ProtoMember(3)]
        public DateTime DateTime { get; set; }
    }
}
