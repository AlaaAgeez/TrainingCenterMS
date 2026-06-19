using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Users;

namespace TrainingCenter.Business.Validators.Users
{
    public class ChangeUserRoleValidator : AbstractValidator<ChangeUserRoleDto>
    {
        public ChangeUserRoleValidator()
        {
            RuleFor(x => x.NewRoleId)
                .NotEmpty().WithMessage("New Role ID is required.")
                .InclusiveBetween((byte)1, (byte)255).WithMessage("Invalid Role ID. It must be between 1 and 255.");
        }
    }
}
