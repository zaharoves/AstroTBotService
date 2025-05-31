using ProtoBuf;

namespace AstroTBotService.RMQ
{
    [ProtoContract]
    public class DailyForecastAspect
    {
        [ProtoMember(1)]
        public string NatalPlanet { get; set; }

        [ProtoMember(2)]
        public double NatalZodiacAngles { get; set; }

        [ProtoMember(3)]
        public string NatalZodiac { get; set; }

        [ProtoMember(4)]
        public bool IsNatalRetro { get; set; }


        [ProtoMember(5)]
        public string TransitPlanet { get; set; }

        [ProtoMember(6)]
        public double TransitZodiacAngles { get; set; }

        [ProtoMember(7)]
        public string TransitZodiac { get; set; }

        [ProtoMember(8)]
        public bool IsTransitRetro { get; set; }


        [ProtoMember(9)]
        public string Aspect { get; set; }
    }
}
