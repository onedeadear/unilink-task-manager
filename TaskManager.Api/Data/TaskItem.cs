using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Data
{
    public class TaskItem
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public required string Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime DueDate { get; set; } // Currently no validation is done on the due date but would be a prime candidate for validation given the business rules
        
        public bool IsCompleted { get; set; }
    }
}