using Microsoft.EntityFrameworkCore;

namespace TaskManager.Api.Data
{
    public class TaskManagerDbContext(DbContextOptions<TaskManagerDbContext> options) : DbContext(options)
    {
        public DbSet<TaskItem> Tasks { get; set; }
    }
}