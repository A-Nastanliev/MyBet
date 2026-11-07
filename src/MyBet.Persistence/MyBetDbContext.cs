using Microsoft.EntityFrameworkCore;
using MyBet.Persistence.Models;

namespace MyBet.Persistence
{
    public class MyBetDbContext : DbContext
    {
        public DbSet<SlipRecord> Slips => Set<SlipRecord>();
        public DbSet<ScannedFileRecord> ScannedFiles => Set<ScannedFileRecord>();
        public DbSet<ReviewItemRecord> ReviewQueue => Set<ReviewItemRecord>();

        public MyBetDbContext(DbContextOptions<MyBetDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyBetDbContext).Assembly);
    }
}
