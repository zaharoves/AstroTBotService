using AstroHandlerService.Entities;
using AstroTBotService.Enums;
using ProtoBuf;

namespace AstroTBotService.RMQ.Entities
{
    [ProtoContract]
    public class AspectInfo
    {
        [ProtoMember(1)]
        public PlanetInfo NatalPlanet { get; set; }

        [ProtoMember(2)]
        public PlanetInfo TransitPlanet { get; set; }

        [ProtoMember(3)]
        public AspectEnum Aspect { get; set; }

        public AspectInfo(PlanetInfo natalPlanet, PlanetInfo transitPlanet, AspectEnum aspect)
        {
            NatalPlanet = natalPlanet;
            TransitPlanet = transitPlanet;
            Aspect = aspect;
        }
    }
}
