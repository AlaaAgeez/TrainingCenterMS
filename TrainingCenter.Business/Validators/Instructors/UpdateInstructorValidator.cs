using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Instructors;

namespace TrainingCenter.Business.Validators.Instructors
{
    public class UpdateInstructorValidator : AbstractValidator<UpdateInstructorDto>
    {
        public UpdateInstructorValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^01[0-2,5]{1}[0-9]{8}$").WithMessage("Invalid phone number format.");

            RuleFor(x => x.Salary)
                .NotEmpty().WithMessage("Salary is required.")
                .GreaterThanOrEqualTo(0).WithMessage("Salary cannot be negative.")
                .ScalePrecision(2, 10).WithMessage("Salary must be within range (10,2).");
        }
    }
}
