
using AstroTBotService.Db.Entities;
using Microsoft.EntityFrameworkCore;

namespace AstroTBotService.Db.Providers
{
    internal class EphemerisProvider : IEphemerisProvider
    {
        private ApplicationContext _appContext { get; set; }

        public EphemerisProvider(
            ApplicationContext appContext)
        {
            _appContext = appContext;
        }

        //TODO
        public async Task AddEphemerises(IEnumerable<Ephemeris> ephemerises)
        {
            await _appContext.Ephemerises.AddRangeAsync(ephemerises);
            await _appContext.SaveChangesAsync();

            // получаем объекты из бд и выводим на консоль
            var ephs = await _appContext.Ephemerises.ToListAsync();


            //Console.WriteLine("Users list:");
            foreach (Ephemeris eph in ephs)
            {
                //Console.WriteLine($"{eph.Id}.{eph.DateTime} - {eph.SunDegrees}");
            }

        }
    }
}
