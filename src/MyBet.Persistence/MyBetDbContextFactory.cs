using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MyBet.Persistence
{
    public class MyBetDbContextFactory : IDesignTimeDbContextFactory<MyBetDbContext>
    {
        public MyBetDbContext CreateDbContext(string[] args) =>
            new(new DbContextOptionsBuilder<MyBetDbContext>().UseSqlite("Data Source=design-time.db").Options);
    }
}