
using AstroTBotService.Db.Entities;

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

        public void AddEphemerises(IEnumerable<Ephemeris> ephemerises)
        {
            _appContext.Ephemerises.AddRange(ephemerises);
            _appContext.SaveChanges();

            // получаем объекты из бд и выводим на консоль
            var ephs = _appContext.Ephemerises.ToList();


            //Console.WriteLine("Users list:");
            foreach (Ephemeris eph in ephs)
            {
                //Console.WriteLine($"{eph.Id}.{eph.DateTime} - {eph.SunDegrees}");
            }

        }
    }
}
