using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Models
{
    public class UpdateTaskRequest
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Title { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime DueDate { get; set; }

        public bool IsCompleted { get; set; }
    }
}
