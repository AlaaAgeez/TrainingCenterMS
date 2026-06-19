using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Instructors;

namespace TrainingCenter.Business.Validators.Instructors
{
    public class GetInstructorsRequestValidator : AbstractValidator<GetInstructorsRequestDto>
    {
        public GetInstructorsRequestValidator()
        {
            RuleFor(x => x.InstructorId)
                .GreaterThan(0)
                .When(x => x.InstructorId > 0)
                .WithMessage("Invalid Instructor ID.");

            RuleFor(x => x.UserId)
                .GreaterThan(0)
                .When(x => x.UserId > 0)
                .WithMessage("Invalid User ID.");

            RuleFor(x => x.Salary)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Salary > 0)
                .WithMessage("Salary cannot be negative.");

            RuleFor(x => x.Email)
                .EmailAddress()
                .When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("Invalid email format.");
        }
    }
}
