using TaskManager.Api.Models;

namespace TaskManager.Api.Data
{
    public interface ITaskRepository
    {
        Task<IEnumerable<TaskResponse>> GetTasksAsync(string? title = null, bool? isCompleted = null, int page = 1, int pageSize = 10, SortOptions sortBy = SortOptions.DueDate, SortOrder sortOrder = SortOrder.Ascending);

        Task<TaskResponse?> GetTaskAsync(int id);

        Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request);

        Task<bool> UpdateTaskAsync(UpdateTaskRequest request);

        Task<bool> DeleteTaskAsync(int id);
    }
}
