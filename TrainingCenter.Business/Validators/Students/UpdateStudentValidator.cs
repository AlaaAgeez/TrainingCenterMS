using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Students;

namespace TrainingCenter.Business.Validators.Students
{
    public class UpdateStudentValidator : AbstractValidator<UpdateStudentDto>
    {
        public UpdateStudentValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(x => x == "Active" || x == "Inactive" || x == "Suspended")
                .WithMessage("Invalid status. Must be 'Active', 'Inactive', or 'Suspended'.");
        }
    }
}
