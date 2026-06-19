using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Auth;

namespace TrainingCenter.Business.Validators.Auth
{
    public class RegisterStudentValidator : AbstractValidator<RegisterStudentDto>
    {
        public RegisterStudentValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email address.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Must be at least 8 characters.")
                .Matches("[A-Z]").WithMessage("Must contain uppercase letter.")
                .Matches("[0-9]").WithMessage("Must contain a number.");
        }
    }
}
