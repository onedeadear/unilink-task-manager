using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Data;
using TaskManager.Api.Models;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController(ITaskRepository repository, ILogger<TasksController> logger) : ControllerBase
    {
        private readonly ITaskRepository _repository = repository;

        private readonly ILogger<TasksController> _logger = logger;

        /// <summary>
        /// Gets a paginated list of tasks, optionally filtered by title and completion status, and sorted by a specified field.
        /// </summary>
        /// <param name="title">Filter by title substring.</param>
        /// <param name="isCompleted">Filter by completion status.</param>
        /// <param name="page">Page number (default 1).</param>
        /// <param name="pageSize">Page size (default 10).</param>
        /// <param name="sortBy">Field to sort by: Id, Title, DueDate, IsCompleted (default: DueDate).</param>
        /// <param name="sortOrder">Sort order: Ascending or Descending (default: Ascending).</param>
        /// <returns>List of tasks.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskResponse>>> GetTasks(
            [FromQuery] string? title = null,
            [FromQuery] bool? isCompleted = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = null)
        {

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            const int MaxPageSize = 100;
            if (pageSize > MaxPageSize)
            {
                _logger.LogWarning("Requested pageSize {PageSize} exceeds maximum allowed {MaxPageSize}", pageSize, MaxPageSize);
                return BadRequest($"Maximum page size is {MaxPageSize}.");
            }

            // Parse and validate sortBy parameter
            SortOptions sortByOption = SortOptions.DueDate;
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (!Enum.TryParse<SortOptions>(sortBy, ignoreCase: true, out sortByOption))
                {
                    _logger.LogWarning("Invalid sortBy value: {SortBy}", sortBy);
                    return BadRequest($"Invalid sortBy value. Valid options are: {string.Join(", ", Enum.GetNames<SortOptions>())}.");
                }
            }

            // Parse and validate sortOrder parameter
            SortOrder sortOrderOption = SortOrder.Ascending;
            if (!string.IsNullOrWhiteSpace(sortOrder))
            {
                if (!Enum.TryParse<SortOrder>(sortOrder, ignoreCase: true, out sortOrderOption))
                {
                    _logger.LogWarning("Invalid sortOrder value: {SortOrder}", sortOrder);
                    return BadRequest($"Invalid sortOrder value. Valid options are: {string.Join(", ", Enum.GetNames<SortOrder>())}.");
                }
            }
            
            var tasks = await _repository.GetTasksAsync(title, isCompleted, page, pageSize, sortByOption, sortOrderOption);
            return Ok(tasks);
        }

        /// <summary>
        /// Gets a single task by its ID.
        /// </summary>
        /// <param name="id">Task ID.</param>
        /// <returns>The requested task, or 404 if not found.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskResponse>> GetTask(int id)
        {
            var task = await _repository.GetTaskAsync(id);

            if (task == null)
            {
                _logger.LogWarning("Task with id {Id} not found", id);
                return NotFound();
            }

            return Ok(task);
        }

        /// <summary>
        /// Creates a new task.
        /// </summary>
        /// <param name="request">Task creation request.</param>
        /// <returns>The created task.</returns>
        [HttpPost]
        public async Task<ActionResult<TaskResponse>> CreateTask([FromBody] CreateTaskRequest request)
        {
            var createdTask = await _repository.CreateTaskAsync(request);
            return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id }, createdTask);
        }

        /// <summary>
        /// Updates an existing task.
        /// </summary>
        /// <param name="id">Task ID.</param>
        /// <param name="request">Task update request.</param>
        /// <returns>No content if successful, 404 or 400 otherwise.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskRequest request)
        {
            if (id != request.Id)
            {
                _logger.LogWarning("Route id {RouteId} does not match request id {RequestId}", id, request.Id);
                return BadRequest("Route id and request id do not match.");
            }

            var success = await _repository.UpdateTaskAsync(request);
            if (!success)
            {
                _logger.LogWarning("Task with id {Id} not found for update", id);
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a task by its ID.
        /// </summary>
        /// <param name="id">Task ID.</param>
        /// <returns>No content if successful, 404 otherwise.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var success = await _repository.DeleteTaskAsync(id);
            if (!success)
            {
                _logger.LogWarning("Task with id {Id} not found for deletion", id);
                return NotFound();
            }
            
            _logger.LogInformation("Task with id {Id} deleted successfully", id);
            return NoContent();
        }
    }
}
