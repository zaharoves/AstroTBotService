using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AstroTBotService.Db.Entities;
using AstroTBotService.Configurations;

namespace AstroTBotService.Db
{
    public class ApplicationContext : DbContext
    {
        private PostgresConfig _postgresConfig;

        public DbSet<Ephemeris> Ephemerises { get; }
        public DbSet<AstroUser> AstroUsers { get; }
        public DbSet<UserStage> UsersStages { get; }

        public ApplicationContext(IOptions<PostgresConfig> postgresConfig)
        {
            _postgresConfig = postgresConfig.Value;

            Ephemerises = Set<Ephemeris>();
            AstroUsers = Set<AstroUser>();
            UsersStages = Set<UserStage>();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_postgresConfig.ConnectionString);
        }
    }
}