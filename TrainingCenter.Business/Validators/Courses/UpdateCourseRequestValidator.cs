using FluentValidation;
using TrainingCenter.Core.DTOs.Courses;

namespace TrainingCenter.Core.Validators
{
    public class UpdateCourseRequestValidator : AbstractValidator<UpdateCourseRequestDto>
    {
        public UpdateCourseRequestValidator()
        {
            RuleFor(x => x.Title).MaximumLength(150).When(x => x.Title != null);
            RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description != null);
            RuleFor(x => x.Price).InclusiveBetween(0, 50000).When(x => x.Price.HasValue);

            RuleFor(x => x.Level)
                .Must(x => new[] { "Advanced", "Intermediate", "Beginner" }.Contains(x))
                .When(x => !string.IsNullOrEmpty(x.Level));

            RuleFor(x => x.DurationHours).InclusiveBetween(1, 500).When(x => x.DurationHours.HasValue);

            RuleFor(x => x.Status)
                .Must(x => new[] { "Archived", "Published", "Draft" }.Contains(x))
                .When(x => !string.IsNullOrEmpty(x.Status));

            RuleFor(x => x.InstructorId).GreaterThan(0).When(x => x.InstructorId.HasValue);
        }
    }
}