using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Courses;

namespace TrainingCenter.Business.Validators.Courses
{
    public class CreateCourseRequestValidator : AbstractValidator<CreateCourseRequestDto>
    {
        public CreateCourseRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Course title is required.")
                .MaximumLength(150).WithMessage("Title cannot exceed 150 characters.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Course code is required.")
                .MaximumLength(30).WithMessage("Code cannot exceed 30 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be a positive number.");

            RuleFor(x => x.Level)
                .Must(x => new[] { "Advanced", "Intermediate", "Beginner" }.Contains(x))
                .WithMessage("Level must be 'Advanced', 'Intermediate', or 'Beginner'.");

            RuleFor(x => x.DurationHours)
                .InclusiveBetween(1, 500).WithMessage("Duration must be between 1 and 500 hours.");

            RuleFor(x => x.Status)
                  .Must(x => new[] { "Archived", "Published", "Draft" }.Contains(x))
                  .WithMessage("Status must be 'Archived', 'Published', or 'Draft'.");
        }
    }
}
