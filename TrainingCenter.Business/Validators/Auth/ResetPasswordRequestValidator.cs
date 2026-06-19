using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Auth;

namespace TrainingCenter.Business.Validators.Auth
{
    public class ResetPasswordRequestValidator: AbstractValidator<ResetPasswordRequestDto>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.Email).
                NotEmpty().WithMessage("Email is required.").
                EmailAddress().WithMessage("Invalid email address.");

            RuleFor(x => x.Otp).NotEmpty().WithMessage("OTP is required.").
                Length(6).WithMessage("OTP must be 6 digits.");

            RuleFor(x => x.NewPassword).
                NotEmpty().MinimumLength(8).Matches("[A-Z]").WithMessage("Must contain uppercase letter").
                Matches("[0-9]").WithMessage("Must contain a number");

            RuleFor(x => x.ConfirmNewPassword)
                .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
        }
    }
}
