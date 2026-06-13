using AI.CustomerSupport.API.Data;
using Microsoft.EntityFrameworkCore;

namespace AI.CustomerSupport.Tests.TestSupport
{
    internal static class TestDbContextFactory
    {
        public static AppDbContext Create(string databaseName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            return new AppDbContext(options);
        }
    }
}
