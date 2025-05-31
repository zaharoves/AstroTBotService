using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AstroHandlerService.Configurations;
using AstroHandlerService.Db.Entities;
using AstroTBotService.Db.Entities;

namespace AstroHandlerService.Db
{
    public class ApplicationContext : DbContext
    {
        private PostgresConfig _postgresConfig;

        public DbSet<Ephemeris> Ephemerises { get; }
        public DbSet<User> Users { get; }
        public DbSet<UserStage> UsersStages { get; }

        public ApplicationContext(IOptions<PostgresConfig> postgresConfig)
        {
            _postgresConfig = postgresConfig.Value;

            Ephemerises = Set<Ephemeris>();
            Users = Set<User>();
            UsersStages = Set<UserStage>();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_postgresConfig.ConnectionString);
        }
    }
}