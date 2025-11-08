using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using TaskManager.Api.Models;

namespace TaskManager.Tests
{
    public class TaskRequestValidationTests
    {
        [TestCase("")]
        [TestCase("  ")]
        [TestCase(null)]
        public void CreateTaskRequest_TitleIsRequired_ShouldFailValidation(string? title)
        {
            var dto = new CreateTaskRequest
            {
                Title = title!, // Invalid
                Description = "Desc",
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(dto);
            var isValid = Validator.TryValidateObject(dto, context, results, true);

            isValid.Should().BeFalse();
            results.Should().Contain(r => r.MemberNames.Contains("Title"));
        }

        [Test]
        public void CreateTaskRequest_TitleTooLong_ShouldFailValidation()
        {
            var dto = new CreateTaskRequest
            {
                Title = new string('A', 101), // Exceeds max length
                Description = "Desc",
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(dto);
            var isValid = Validator.TryValidateObject(dto, context, results, true);

            isValid.Should().BeFalse();
            results.Should().Contain(r => r.MemberNames.Contains("Title"));
        }

        [Test]
        public void CreateTaskRequest_DescriptionTooLong_ShouldFailValidation()
        {
            var dto = new CreateTaskRequest
            {
                Title = "Valid Title",
                Description = new string('B', 501), // Exceeds max length
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(dto);
            var isValid = Validator.TryValidateObject(dto, context, results, true);

            isValid.Should().BeFalse();
            results.Should().Contain(r => r.MemberNames.Contains("Description"));
        }

        [Test]
        public void CreateTaskRequest_Valid_ShouldPassValidation()
        {
            var dto = new CreateTaskRequest
            {
                Title = "Valid Title",
                Description = "Desc",
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(dto);
            var isValid = Validator.TryValidateObject(dto, context, results, true);

            isValid.Should().BeTrue();
            results.Should().BeEmpty();
        }

        [Test]
        public void UpdateTaskRequest_TitleIsRequired_ShouldFailValidation()
        {
            var dto = new UpdateTaskRequest
            {
                Title = null!, // Invalid
                Description = "Desc",
                DueDate = DateTime.UtcNow.AddDays(1),
                IsCompleted = false
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(dto);
            var isValid = Validator.TryValidateObject(dto, context, results, true);

            isValid.Should().BeFalse();
            results.Should().Contain(r => r.MemberNames.Contains("Title"));
        }

        [Test]
        public void UpdateTaskRequest_TitleTooLong_ShouldFailValidation()
        {
            var dto = new UpdateTaskRequest
            {
                Title = new string('A', 101), // Exceeds max length
                Description = "Desc",
                DueDate = DateTime.UtcNow.AddDays(1),
                IsCompleted = false
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(dto);
            var isValid = Validator.TryValidateObject(dto, context, results, true);

            isValid.Should().BeFalse();
            results.Should().Contain(r => r.MemberNames.Contains("Title"));
        }

        [Test]
        public void UpdateTaskRequest_DescriptionTooLong_ShouldFailValidation()
        {
            var dto = new UpdateTaskRequest
            {
                Title = "Valid Title",
                Description = new string('B', 501), // Exceeds max length
                DueDate = DateTime.UtcNow.AddDays(1),
                IsCompleted = false
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(dto);
            var isValid = Validator.TryValidateObject(dto, context, results, true);

            isValid.Should().BeFalse();
            results.Should().Contain(r => r.MemberNames.Contains("Description"));
        }

        [Test]
        public void UpdateTaskRequest_Valid_ShouldPassValidation()
        {
            var dto = new UpdateTaskRequest
            {
                Title = "Valid Title",
                Description = "Desc",
                DueDate = DateTime.UtcNow.AddDays(1),
                IsCompleted = false
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(dto);
            var isValid = Validator.TryValidateObject(dto, context, results, true);

            isValid.Should().BeTrue();
            results.Should().BeEmpty();
        }
    }
}