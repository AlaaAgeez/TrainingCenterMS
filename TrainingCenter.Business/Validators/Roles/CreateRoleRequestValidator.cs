using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Roles;

namespace TrainingCenter.Business.Validators.Roles
{
    public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequestDto>
    {
        public CreateRoleRequestValidator()
        {
            RuleFor(x => x.RoleName)
                .NotEmpty().WithMessage("Role name is required.")
                .MinimumLength(2).WithMessage("Role name must be at least 2 characters.")
                .MaximumLength(50).WithMessage("Role name must not exceed 50 characters.")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Role name must contain letters only.");
        }
    }
}
