using TaskManager.Api.Controllers;
using TaskManager.Api.Data;
using TaskManager.Api.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using FluentAssertions;

namespace TaskManager.Tests
{
    public class TasksTests : BaseTestContext
    {
        private TaskRepository _repository;

        private TasksController _controller;

        [OneTimeSetUp]
        public void Setup()
        {
            _repository = new TaskRepository(Context);
            _controller = new TasksController(_repository, Substitute.For<ILogger<TasksController>>());
        }

        [TearDown]
        public void Teardown()
        {
            Context.Tasks.RemoveRange(Context.Tasks);
            Context.SaveChanges();
        }

        [Test]
        public async Task GetTasks_ReturnsAllTasks()
        {
            // Arrange
            Context.Tasks.Add(new TaskItem
            {
                Title = "Test Task",
                Description = "Desc",
                DueDate = DateTime.UtcNow.AddDays(1),
                IsCompleted = false
            });
            
            Context.Tasks.Add(new TaskItem
            {
                Title = "Second Task",
                Description = "Another Desc",
                DueDate = DateTime.UtcNow.AddDays(2),
                IsCompleted = true
            });
            Context.SaveChanges();

            // Act
            var result = await _controller.GetTasks();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();

            var tasks = okResult.Value as IEnumerable<TaskResponse>;
            tasks.Should().NotBeNull();
            tasks.Should().HaveCount(2);
        }

        [Test]
        public async Task GetTasks_Paging_WorksCorrectly()
        {
            // Arrange: Add 15 tasks
            for (int i = 1; i <= 15; i++)
            {
                Context.Tasks.Add(new TaskItem
                {
                    Title = $"Task {i}",
                    Description = $"Desc {i}",
                    DueDate = DateTime.UtcNow.AddDays(i),
                    IsCompleted = false
                });
            }
            Context.SaveChanges();

            // Act: Request page 2, pageSize 10
            var result = await _controller.GetTasks(null, null, 2, 10);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();

            var tasks = okResult.Value as IEnumerable<TaskResponse>;
            tasks.Should().NotBeNull();
            tasks.Should().HaveCount(5);
            tasks!.First().Title.Should().Be("Task 11");
        }

        [Test]
        public async Task GetTasks_FilterByCompleted_WorksCorrectly()
        {
            // Arrange
            Context.Tasks.Add(new TaskItem { Title = "A", Description = "Desc", DueDate = DateTime.UtcNow, IsCompleted = true });
            Context.Tasks.Add(new TaskItem { Title = "B", Description = "Desc", DueDate = DateTime.UtcNow, IsCompleted = false });
            Context.SaveChanges();

            // Act
            var result = await _controller.GetTasks(null, true);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();

            var tasks = okResult.Value as IEnumerable<TaskResponse>;
            tasks.Should().NotBeNull();
            tasks.Should().HaveCount(1);
            tasks!.First().IsCompleted.Should().BeTrue();
        }

        [Test]
        public async Task GetTasks_FilterByTitle_WorksCorrectly()
        {
            // Arrange
            Context.Tasks.Add(new TaskItem { Title = "Alpha", Description = "Desc", DueDate = DateTime.UtcNow, IsCompleted = false });
            Context.Tasks.Add(new TaskItem { Title = "Beta", Description = "Desc", DueDate = DateTime.UtcNow, IsCompleted = false });
            Context.SaveChanges();

            // Act
            var result = await _controller.GetTasks("Al", null);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            
            var tasks = okResult.Value as IEnumerable<TaskResponse>;
            tasks.Should().NotBeNull();
            tasks.Should().HaveCount(1);
            tasks!.First().Title.Should().Be("Alpha");
        }
        
        [Test]
        public async Task GetTasks_ReturnsBadRequest_WhenPageSizeExceedsMax()
        {
            // Arrange
            int tooLargePageSize = 101; // MaxPageSize is 100

            // Act
            var result = await _controller.GetTasks(null, null, 1, tooLargePageSize);

            // Assert
            var badRequest = result.Result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.Value.Should().Be("Maximum page size is 100.");
        }

        [Test]
        public async Task GetTasks_SortByDueDate_Ascending_WorksCorrectly()
        {
            // Arrange
            var now = DateTime.UtcNow;
            Context.Tasks.Add(new TaskItem { Title = "Task 1", Description = "Desc", DueDate = now.AddDays(3), IsCompleted = false });
            Context.Tasks.Add(new TaskItem { Title = "Task 2", Description = "Desc", DueDate = now.AddDays(1), IsCompleted = false });
            Context.Tasks.Add(new TaskItem { Title = "Task 3", Description = "Desc", DueDate = now.AddDays(2), IsCompleted = false });
            Context.SaveChanges();

            // Act
            var result = await _controller.GetTasks(null, null, 1, 10, "DueDate", "Ascending");

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();

            var tasks = okResult.Value as IEnumerable<TaskResponse>;
            tasks.Should().NotBeNull();
            tasks.Should().HaveCount(3);
            tasks!.ElementAt(0).DueDate.Should().Be(now.AddDays(1));
            tasks.ElementAt(1).DueDate.Should().Be(now.AddDays(2));
            tasks.ElementAt(2).DueDate.Should().Be(now.AddDays(3));
        }

        [Test]
        public async Task GetTasks_SortByDueDate_Descending_WorksCorrectly()
        {
            // Arrange
            var now = DateTime.UtcNow;
            Context.Tasks.Add(new TaskItem { Title = "Task 1", Description = "Desc", DueDate = now.AddDays(1), IsCompleted = false });
            Context.Tasks.Add(new TaskItem { Title = "Task 2", Description = "Desc", DueDate = now.AddDays(3), IsCompleted = false });
            Context.Tasks.Add(new TaskItem { Title = "Task 3", Description = "Desc", DueDate = now.AddDays(2), IsCompleted = false });
            Context.SaveChanges();

            // Act
            var result = await _controller.GetTasks(null, null, 1, 10, "DueDate", "Descending");

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();

            var tasks = okResult.Value as IEnumerable<TaskResponse>;
            tasks.Should().NotBeNull();
            tasks.Should().HaveCount(3);
            tasks!.ElementAt(0).DueDate.Should().Be(now.AddDays(3));
            tasks.ElementAt(1).DueDate.Should().Be(now.AddDays(2));
            tasks.ElementAt(2).DueDate.Should().Be(now.AddDays(1));
        }

        [Test]
        public async Task GetTasks_SortByTitle_Ascending_WorksCorrectly()
        {
            // Arrange
            Context.Tasks.Add(new TaskItem { Title = "Zebra", Description = "Desc", DueDate = DateTime.UtcNow, IsCompleted = false });
            Context.Tasks.Add(new TaskItem { Title = "Alpha", Description = "Desc", DueDate = DateTime.UtcNow, IsCompleted = false });
            Context.Tasks.Add(new TaskItem { Title = "Beta", Description = "Desc", DueDate = DateTime.UtcNow, IsCompleted = false });
            Context.SaveChanges();

            // Act
            var result = await _controller.GetTasks(null, null, 1, 10, "Title", "Ascending");

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();

            var tasks = okResult.Value as IEnumerable<TaskResponse>;
            tasks.Should().NotBeNull();
            tasks.Should().HaveCount(3);
            tasks!.ElementAt(0).Title.Should().Be("Alpha");
            tasks.ElementAt(1).Title.Should().Be("Beta");
            tasks.ElementAt(2).Title.Should().Be("Zebra");
        }

        [Test]
        public async Task GetTasks_SortByTitle_Descending_WorksCorrectly()
        {
            // Arrange
            Context.Tasks.Add(new TaskItem { Title = "Alpha", Description = "Desc", DueDate = DateTime.UtcNow, IsCompleted = false });
            Context.Tasks.Add(new TaskItem { Title = "Zebra", Description = "Desc", DueDate = DateTime.UtcNow, IsCompleted = false });
            Context.Tasks.Add(new TaskItem { Title = "Beta", Description = "Desc", DueDate = DateTime.UtcNow, IsCompleted = false });
            Context.SaveChanges();

            // Act
            var result = await _controller.GetTasks(null, null, 1, 10, "Title", "Descending");

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();

            var tasks = okResult.Value as IEnumerable<TaskResponse>;
            tasks.Should().NotBeNull();
            tasks.Should().HaveCount(3);
            tasks!.ElementAt(0).Title.Should().Be("Zebra");
            tasks.ElementAt(1).Title.Should().Be("Beta");
            tasks.ElementAt(2).Title.Should().Be("Alpha");
        }

        [Test]
        public async Task GetTasks_SortById_Ascending_WorksCorrectly()
        {
            // Arrange
            var task1 = new TaskItem { Title = "Task 1", Description = "Desc", DueDate = DateTime.UtcNow, IsCompleted = false };
            var task2 = new TaskItem { Title = "Task 2", Description = "Desc", DueDate = DateTime.UtcNow, IsCompleted = false };
            var task3 = new TaskItem { Title = "Task 3", Description = "Desc", DueDate = DateTime.UtcNow, IsCompleted = false };
            Context.Tasks.Add(task1);
            Context.Tasks.Add(task2);
            Context.Tasks.Add(task3);
            Context.SaveChanges();

            // Act
            var result = await _controller.GetTasks(null, null, 1, 10, "Id", "Ascending");

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();

            var tasks = okResult.Value as IEnumerable<TaskResponse>;
            tasks.Should().NotBeNull();
            tasks.Should().HaveCount(3);
            tasks!.ElementAt(0).Id.Should().BeLessThan(tasks.ElementAt(1).Id);
            tasks.ElementAt(1).Id.Should().BeLessThan(tasks.ElementAt(2).Id);
        }

        [Test]
        public async Task GetTasks_SortByIsCompleted_Ascending_WorksCorrectly()
        {
            // Arrange
            Context.Tasks.Add(new TaskItem { Title = "Task 1", Description = "Desc", DueDate = DateTime.UtcNow, IsCompleted = true });
            Context.Tasks.Add(new TaskItem { Title = "Task 2", Description = "Desc", DueDate = DateTime.UtcNow, IsCompleted = false });
            Context.Tasks.Add(new TaskItem { Title = "Task 3", Description = "Desc", DueDate = DateTime.UtcNow, IsCompleted = true });
            Context.SaveChanges();

            // Act
            var result = await _controller.GetTasks(null, null, 1, 10, "IsCompleted", "Ascending");

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();

            var tasks = okResult.Value as IEnumerable<TaskResponse>;
            tasks.Should().NotBeNull();
            tasks.Should().HaveCount(3);
            tasks!.ElementAt(0).IsCompleted.Should().BeFalse();
            tasks.ElementAt(1).IsCompleted.Should().BeTrue();
            tasks.ElementAt(2).IsCompleted.Should().BeTrue();
        }

        [Test]
        public async Task GetTasks_SortByIsCompleted_Descending_WorksCorrectly()
        {
            // Arrange
            Context.Tasks.Add(new TaskItem { Title = "Task 1", Description = "Desc", DueDate = DateTime.UtcNow, IsCompleted = false });
            Context.Tasks.Add(new TaskItem { Title = "Task 2", Description = "Desc", DueDate = DateTime.UtcNow, IsCompleted = true });
            Context.Tasks.Add(new TaskItem { Title = "Task 3", Description = "Desc", DueDate = DateTime.UtcNow, IsCompleted = false });
            Context.SaveChanges();

            // Act
            var result = await _controller.GetTasks(null, null, 1, 10, "IsCompleted", "Descending");

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();

            var tasks = okResult.Value as IEnumerable<TaskResponse>;
            tasks.Should().NotBeNull();
            tasks.Should().HaveCount(3);
            tasks!.ElementAt(0).IsCompleted.Should().BeTrue();
            tasks.ElementAt(1).IsCompleted.Should().BeFalse();
            tasks.ElementAt(2).IsCompleted.Should().BeFalse();
        }

        [Test]
        public async Task GetTasks_DefaultSort_IsDueDateAscending()
        {
            // Arrange
            var now = DateTime.UtcNow;
            Context.Tasks.Add(new TaskItem { Title = "Task 1", Description = "Desc", DueDate = now.AddDays(3), IsCompleted = false });
            Context.Tasks.Add(new TaskItem { Title = "Task 2", Description = "Desc", DueDate = now.AddDays(1), IsCompleted = false });
            Context.Tasks.Add(new TaskItem { Title = "Task 3", Description = "Desc", DueDate = now.AddDays(2), IsCompleted = false });
            Context.SaveChanges();

            // Act - No sort parameters provided
            var result = await _controller.GetTasks();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();

            var tasks = okResult.Value as IEnumerable<TaskResponse>;
            tasks.Should().NotBeNull();
            tasks.Should().HaveCount(3);
            tasks!.ElementAt(0).DueDate.Should().Be(now.AddDays(1));
            tasks.ElementAt(1).DueDate.Should().Be(now.AddDays(2));
            tasks.ElementAt(2).DueDate.Should().Be(now.AddDays(3));
        }

        [Test]
        public async Task GetTasks_ReturnsBadRequest_WhenInvalidSortBy()
        {
            // Act
            var result = await _controller.GetTasks(null, null, 1, 10, "InvalidField", "Ascending");

            // Assert
            var badRequest = result.Result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.Value.Should().BeAssignableTo<string>();
            badRequest.Value.ToString().Should().Contain("Invalid sortBy value");
        }

        [Test]
        public async Task GetTasks_ReturnsBadRequest_WhenInvalidSortOrder()
        {
            // Act
            var result = await _controller.GetTasks(null, null, 1, 10, "DueDate", "InvalidOrder");

            // Assert
            var badRequest = result.Result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.Value.Should().BeAssignableTo<string>();
            badRequest.Value.ToString().Should().Contain("Invalid sortOrder value");
        }

        [Test]
        public async Task GetTask_ReturnsTask_WhenFound()
        {
            var created = await _repository.CreateTaskAsync(new CreateTaskRequest
            {
                Title = "Find Me",
                Description = "Desc",
                DueDate = DateTime.UtcNow.AddDays(2),
                IsCompleted = false
            });

            var result = await _controller.GetTask(created.Id);

            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();

            var task = okResult.Value as TaskResponse;
            task.Should().NotBeNull();
            task.Id.Should().Be(created.Id);
            task.Description.Should().Be(created.Description);
            task.DueDate.Should().Be(created.DueDate);
            task.IsCompleted.Should().Be(created.IsCompleted);
        }

        [Test]
        public async Task GetTask_ReturnsNotFound_WhenMissing()
        {
            var result = await _controller.GetTask(999);
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task CreateTask_ReturnsCreatedTask()
        {
            var request = new CreateTaskRequest
            {
                Title = "New Task",
                Description = "Desc",
                DueDate = DateTime.UtcNow.AddDays(3),
                IsCompleted = false
            };

            var result = await _controller.CreateTask(request);
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            var task = createdResult.Value as TaskResponse;
            task.Should().NotBeNull();
            task!.Title.Should().Be(request.Title);
        }

        [Test]
        public async Task UpdateTask_ReturnsNoContent_WhenSuccess()
        {
            var created = await _repository.CreateTaskAsync(new CreateTaskRequest
            {
                Title = "Update Me",
                Description = "Desc",
                DueDate = DateTime.UtcNow.AddDays(4),
                IsCompleted = false
            });

            var updateRequest = new UpdateTaskRequest
            {
                Id = created.Id,
                Title = "Updated",
                Description = "Updated Desc",
                DueDate = DateTime.UtcNow.AddDays(5),
                IsCompleted = true
            };

            var result = await _controller.UpdateTask(created.Id, updateRequest);
            result.Should().BeOfType<NoContentResult>();
        }

        [Test]
        public async Task UpdateTask_ReturnsBadRequest_WhenIdMismatch()
        {
            var updateRequest = new UpdateTaskRequest
            {
                Id = 123,
                Title = "Mismatch",
                Description = "Desc",
                DueDate = DateTime.UtcNow.AddDays(5),
                IsCompleted = false
            };

            var result = await _controller.UpdateTask(999, updateRequest);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task UpdateTask_ReturnsNotFound_WhenTaskMissing()
        {
            var updateRequest = new UpdateTaskRequest
            {
                Id = 999,
                Title = "Missing",
                Description = "Desc",
                DueDate = DateTime.UtcNow.AddDays(5),
                IsCompleted = false
            };

            var result = await _controller.UpdateTask(999, updateRequest);
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task DeleteTask_ReturnsNoContent_WhenSuccess()
        {
            var created = await _repository.CreateTaskAsync(new CreateTaskRequest
            {
                Title = "Delete Me",
                Description = "Desc",
                DueDate = DateTime.UtcNow.AddDays(6),
                IsCompleted = false
            });

            var result = await _controller.DeleteTask(created.Id);
            result.Should().BeOfType<NoContentResult>();
        }

        [Test]
        public async Task DeleteTask_ReturnsNotFound_WhenMissing()
        {
            var result = await _controller.DeleteTask(999);
            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
