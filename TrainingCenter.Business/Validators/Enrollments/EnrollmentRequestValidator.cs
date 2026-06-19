using FluentValidation;
using TrainingCenter.Core.DTOs.Enrollments;

namespace TrainingCenter.Business.Validators.Enrollments
{
    public class EnrollmentRequestValidator : AbstractValidator<EnrollmentRequestDto>
    {        
        public EnrollmentRequestValidator()
        {
            RuleFor(x => x.CourseId)
                .NotEmpty().WithMessage("Course ID is required.")
                .GreaterThan(0).WithMessage("Invalid Course ID.");
        }
    }
}