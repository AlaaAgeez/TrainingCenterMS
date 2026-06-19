using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Auth;

namespace TrainingCenter.Business.Validators.Auth
{
    public class ResendVerificationEmailValidator : AbstractValidator<ResendVerificationEmailDto>
    {
        public ResendVerificationEmailValidator()
        {
            RuleFor(x => x.Email).
               NotEmpty().WithMessage("Email is required.").
               EmailAddress().WithMessage("Invalid email address.");
        }
    }
}
