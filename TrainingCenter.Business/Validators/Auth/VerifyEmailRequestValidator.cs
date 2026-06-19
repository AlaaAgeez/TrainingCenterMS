using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Auth;

namespace TrainingCenter.Business.Validators.Auth
{
    public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequestDto>
    {
        public VerifyEmailRequestValidator()
        {
            RuleFor(x => x.Email).
                NotEmpty().WithMessage("Email is required.").
                EmailAddress().WithMessage("Invalid email address.");

            RuleFor(x => x.Otp).NotEmpty().WithMessage("OTP is required.")
                .Length(6).WithMessage("OTP must be 6 digits.");
        }
    }
}
