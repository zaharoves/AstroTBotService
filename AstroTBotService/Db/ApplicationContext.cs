using Microsoft.EntityFrameworkCore;
using AstroTBotService.Db.Entities;

namespace AstroTBotService.Db
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Ephemeris> Ephemerises { get; }
        public DbSet<AstroUser> AstroUsers { get; }
        public DbSet<AstroPerson> AstroPersons { get; }

        public DbSet<UserStage> UsersStages { get; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            Ephemerises = Set<Ephemeris>();
            AstroUsers = Set<AstroUser>();
            AstroPersons = Set<AstroPerson>();
            UsersStages = Set<UserStage>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AstroPerson>()
                .HasOne(e => e.ParentUser)
                .WithMany(a => a.ChildPersons)
                .HasForeignKey(e => e.ParentUserId)
                .OnDelete(DeleteBehavior.SetNull);

            base.OnModelCreating(modelBuilder);
        }
    }
}