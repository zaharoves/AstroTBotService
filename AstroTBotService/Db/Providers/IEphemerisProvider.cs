using AstroTBotService.Db.Entities;

namespace AstroTBotService.Db.Providers
{
    public interface IEphemerisProvider
    {
        public Task AddEphemerises(IEnumerable<Ephemeris> ephemeris);
    }
}
