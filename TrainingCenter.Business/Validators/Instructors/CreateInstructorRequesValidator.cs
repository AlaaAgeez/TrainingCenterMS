using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Instructors;

namespace TrainingCenter.Business.Validators.Instructors
{
    public class CreateInstructorRequesValidator : AbstractValidator<CreateInstructorRequestDto>
    {
        public CreateInstructorRequesValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.")
                .GreaterThan(0).WithMessage("User ID must be greater than zero.");

            RuleFor(x => x.HireDate)
                .NotEmpty().WithMessage("Hire date is required.")
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today)).WithMessage("Hire date cannot be in the future.");

            RuleFor(x => x.Salary)
                .GreaterThanOrEqualTo(0).WithMessage("Salary cannot be negative.")
                .ScalePrecision(2, 10).WithMessage("Salary must be within range (10,2).");

            RuleFor(x => x.ManagerId)
                .GreaterThan(0)
                .When(x => x.ManagerId.HasValue)
                .WithMessage("Manager ID must be greater than zero if provided.");
        }
    }
}