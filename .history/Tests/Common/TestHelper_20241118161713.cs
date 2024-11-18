using Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Tests.Common
{
    public static class TestHelper
    {
        public static AppDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb") // Use an in-memory database for tests
                .Options;

            return new AppDbContext(options);
        }
    }
}