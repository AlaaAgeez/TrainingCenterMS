using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.People;

namespace TrainingCenter.Business.Validators.People
{
    public class PersonRequestValidator :AbstractValidator<PersonRequestDto>
    {
        public PersonRequestValidator()
        {
            RuleFor(x => x.NationalNo)
                .NotEmpty().WithMessage("National No Is Required")
                .MaximumLength(20).WithMessage("National No Max Length Is 20")
                .Matches(@"^[0-9]+$").WithMessage("National No Must Be Numbers Only");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First Name Is Required")
                .MaximumLength(50).WithMessage("First Name Max Length Is 50")
                .Matches(@"^[\u0600-\u06FFa-zA-Z\s]+$").WithMessage("First Name Must Be Letters Only");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last Name Is Required")
                .MaximumLength(50).WithMessage("Last Name Max Length Is 50")
                .Matches(@"^[\u0600-\u06FFa-zA-Z\s]+$").WithMessage("Last Name Must Be Letters Only");

            RuleFor(x => x.SecondName)
                .MaximumLength(50).WithMessage("Second Name Max Length Is 50")
                .Matches(@"^[\u0600-\u06FFa-zA-Z\s]+$").WithMessage("Second Name Must Be Letters Only")
                .When(x => !string.IsNullOrEmpty(x.SecondName));

            RuleFor(x => x.ThirdName)
                .MaximumLength(50).WithMessage("Third Name Max Length Is 50")
                .Matches(@"^[\u0600-\u06FFa-zA-Z\s]+$").WithMessage("Third Name Must Be Letters Only")
                .When(x => !string.IsNullOrEmpty(x.ThirdName));

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date Of Birth Is Required")
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Now.AddYears(-16)))
                .WithMessage("Age Must Be At Least 16 Years")
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Now.AddYears(-100)))
                .WithMessage("Age Must Be Less Than 100 Years");


            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Gender Is Required")
                .Must(x => x == 1 || x == 2).WithMessage("Gender Must Be 1 Or 2");

            RuleFor(x => x.Phone)
                .MaximumLength(30).WithMessage("Phone Max Length Is 30")
                .Matches(@"^\+?[0-9]{7,15}$").WithMessage("Phone Number Is Not Valid")
                .When(x => !string.IsNullOrEmpty(x.Phone));

            RuleFor(x => x.NationalityCountryId)
                .NotEmpty().WithMessage("Nationality Is Required")
                .GreaterThan(0).WithMessage("Nationality Is Not Valid");
        }
    }
}
