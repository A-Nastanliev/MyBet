using Microsoft.EntityFrameworkCore;

namespace MyBet.Tests.Persistence
{
    public class MigrationsMatchModelTests
    {
        [Fact]
        public void The_migrations_describe_the_current_model()
        {
            using var db = new SqliteTestDatabase();
            using var context = db.NewContext();

            Assert.False(context.Database.HasPendingModelChanges());
        }
    }
}