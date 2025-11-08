using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;

namespace TaskManager.Tests
{
    public abstract class BaseTestContext
    {
        protected TaskManagerDbContext Context { get; private set; }

        [OneTimeSetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;

            Context = new TaskManagerDbContext(options);
        }

        [OneTimeTearDown]
        public void TearDown() => Context?.Dispose();
    }
}