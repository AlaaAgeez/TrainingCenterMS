using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.Consts;
using TrainingCenter.Core.DTOs.Common;

namespace TrainingCenter.Business.Validators.Common
{
    public class PaginationValidator : AbstractValidator<PaginationRequestDto>
    {
        public PaginationValidator()
        {
            RuleFor(x => x.Limit)
                .NotNull()
                .When(x => x.Page.HasValue)
                .WithMessage("Limit is required when Page is provided");

            RuleFor(x => x.Page)
                .NotNull()
                .When(x => x.Limit.HasValue)
                .WithMessage("Page is required when Limit is provided");

            RuleFor(x => x.Page)
                .GreaterThan(0)
                .When(x => x.Page.HasValue)
                .WithMessage("Page must be greater than 0");

            RuleFor(x => x.Limit)
                .GreaterThan(0)
                .LessThanOrEqualTo(PaginationConsts.MaxLimit)
                .When(x => x.Limit.HasValue)
                .WithMessage($"Limit must be greater than 0 and not exceed {PaginationConsts.MaxLimit}");

            RuleFor(x => x.OrderByDirection)
                .NotNull()
                .When(x => !string.IsNullOrEmpty(x.OrderBy))
                .WithMessage("OrderByDirection is required when OrderBy is provided");

            RuleFor(x => x.OrderBy)
                .NotNull()
                .When(x => !string.IsNullOrEmpty(x.OrderByDirection))
                .WithMessage("OrderBy is required when OrderByDirection is provided");

            RuleFor(x => x.OrderByDirection)
                .Must(x => x.Equals("ASC", StringComparison.OrdinalIgnoreCase) ||
                           x.Equals("DESC", StringComparison.OrdinalIgnoreCase))
                .When(x => !string.IsNullOrEmpty(x.OrderByDirection))
                .WithMessage("OrderByDirection must be 'ASC' or 'DESC' (case-insensitive)");
        }
    }
}
