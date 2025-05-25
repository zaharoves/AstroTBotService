using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using AstroTBotService.Enums;
using ProtoBuf;

namespace AstroHandlerService.Entities
{
    [ProtoContract]
    public class PlanetInfo
    {
        [ProtoMember(1)]
        public PlanetEnum Planet { get; }

        // Эклиптическая долгота.
        // Градус угла планеты относительно 0 гр. Овна.
        // Может принимать значения от 0 до 360
        public double AbsolutAngles { get; }

        [ProtoMember(2)]
        public ZodiacEnum Zodiac { get; }

        [ProtoMember(3)]
        public double ZodiacAngles { get; }

        public double Speed { get; set; }

        [ProtoMember(4)]
        public bool IsRetro => Speed < 0;
       
    }
}
