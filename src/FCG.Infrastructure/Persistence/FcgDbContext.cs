using FCG.Domain.Games;
using FCG.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace FCG.Infrastructure.Persistence
{
    public sealed class FcgDbContext : DbContext
    {
        public FcgDbContext(DbContextOptions<FcgDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Game> Games => Set<Game>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FcgDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
