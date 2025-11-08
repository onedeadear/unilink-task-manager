using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Models;

namespace TaskManager.Api.Data
{
    public class TaskRepository : ITaskRepository
    {
        private readonly TaskManagerDbContext _context;

        public TaskRepository(TaskManagerDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TaskResponse>> GetTasksAsync(string? title = null, bool? isCompleted = null, int page = 1, int pageSize = 10, SortOptions sortBy = SortOptions.DueDate, SortOrder sortOrder = SortOrder.Ascending)
        {
            var query = _context.Tasks.AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(t => t.Title.Contains(title)); // Case sensitive search for in-memory
            }

            if (isCompleted.HasValue)
            {
                query = query.Where(t => t.IsCompleted == isCompleted.Value);
            }

            // Sorting
            query = sortBy switch
            {
                SortOptions.Id => sortOrder == SortOrder.Ascending
                    ? query.OrderBy(t => t.Id)
                    : query.OrderByDescending(t => t.Id),

                SortOptions.Title => sortOrder == SortOrder.Ascending
                    ? query.OrderBy(t => t.Title)
                    : query.OrderByDescending(t => t.Title),

                SortOptions.DueDate => sortOrder == SortOrder.Ascending
                    ? query.OrderBy(t => t.DueDate)
                    : query.OrderByDescending(t => t.DueDate),

                SortOptions.IsCompleted => sortOrder == SortOrder.Ascending
                    ? query.OrderBy(t => t.IsCompleted)
                    : query.OrderByDescending(t => t.IsCompleted),

                _ => query.OrderBy(t => t.DueDate)
            };

            // Paging
            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            var items = await query.ToListAsync();
            return items.Select(MapToResponse);
        }

        public async Task<TaskResponse?> GetTaskAsync(int id)
        {
            var item = await _context.Tasks.FindAsync(id);
            return item == null ? null : MapToResponse(item);
        }

        public async Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request)
        {
            var task = new TaskItem
            {
                Title = request.Title,
                Description = request.Description,
                DueDate = request.DueDate,
                IsCompleted = request.IsCompleted
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return MapToResponse(task);
        }

        private static TaskResponse MapToResponse(TaskItem item)
        {
            return new TaskResponse
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                DueDate = item.DueDate,
                IsCompleted = item.IsCompleted
            };
        }

        public async Task<bool> UpdateTaskAsync(UpdateTaskRequest request)
        {
            var task = await _context.Tasks.FindAsync(request.Id);
            if (task == null)
            {
                return false;
            }

            task.Title = request.Title;
            task.Description = request.Description;
            task.DueDate = request.DueDate;
            task.IsCompleted = request.IsCompleted;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Tasks.Any(e => e.Id == request.Id))
                {
                    return false;
                }

                throw;
            }

            return true;
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return false;
            }

            _context.Tasks.Remove(task);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
