
namespace AstroHandlerService.Db.Entities
{
    public class Ephemeris
    {
        public Ephemeris()
        {
        }

        public long? Id { get; set; }
        public DateTime? DateTime { get; set; }

        public double? SunAngles { get; set; }
        public double? MoonAngles { get; set; }
        public double? MercuryAngles { get; set; }
        public double? VenusAngles { get; set; }
        public double? MarsAngles { get; set; }
        public double? JupiterAngles { get; set; }
        public double? SaturnAngles { get; set; }
        public double? UranAngles { get; set; }
        public double? NeptuneAngles { get; set; }
        public double? PlutoAngles { get; set; }
    }
}
