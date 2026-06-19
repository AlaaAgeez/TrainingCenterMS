using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Users;
namespace TrainingCenter.Business.Validators.Users
{
    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required");

            When(x => !string.IsNullOrEmpty(x.NewPassword) || !string.IsNullOrEmpty(x.ConfirmNewPassword), () =>
            {
                RuleFor(x => x.NewPassword)
                    .NotEmpty().WithMessage("New password is required.")
                    .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                    .Matches("[A-Z]").WithMessage("Must contain uppercase letter")
                    .Matches("[0-9]").WithMessage("Must contain a number");

                RuleFor(x => x.ConfirmNewPassword)
                    .NotEmpty().WithMessage("Confirm password is required.")
                    .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
            });
        }
    }
}
