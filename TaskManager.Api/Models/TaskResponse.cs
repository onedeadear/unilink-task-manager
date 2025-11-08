using System;

namespace TaskManager.Api.Models
{
    public class TaskResponse
    {
        public int Id { get; set; }

        public required string Title { get; set; }

        public string? Description { get; set; }

        public DateTime DueDate { get; set; }

        public bool IsCompleted { get; set; }
    }
}
