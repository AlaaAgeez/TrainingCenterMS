using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Enrollments;

namespace TrainingCenter.Business.Validators.Enrollments
{
    public class UpdateEnrollmentValidator : AbstractValidator<UpdateEnrollmentDto>
    {
        public UpdateEnrollmentValidator()
        {
            RuleFor(x => x.ProgressPercent)
                .InclusiveBetween(0, 100)
                .When(x => x.ProgressPercent.HasValue)
                .WithMessage("Progress must be between 0 and 100.");

            RuleFor(x => x.FinalGrade)
                .InclusiveBetween(0, 100)
                .When(x => x.FinalGrade.HasValue)
                .WithMessage("Final grade must be between 0 and 100.");

            RuleFor(x => x.Status)
                .Must(x => new[] { "Suspended", "Dropped", "Completed", "Enrolled" }
                    .Contains(x, StringComparer.OrdinalIgnoreCase))
                .When(x => !string.IsNullOrEmpty(x.Status))
                .WithMessage("Invalid status. Allowed values: Suspended, Dropped, Completed, Enrolled.");
        }
    }
}
