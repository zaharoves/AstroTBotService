using AstroTBotService.Db.Entities;

namespace AstroTBotService.Db.Providers
{
    public interface IEphemerisProvider
    {
        public void AddEphemerises(IEnumerable<Ephemeris> ephemeris);
    }
}
