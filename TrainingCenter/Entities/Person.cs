using System;
using System.Collections.Generic;

namespace TrainingCenter.Core.Entities;

public partial class Person
{
    public int PersonId { get; set; }

    public string NationalNo { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? SecondName { get; set; }

    public string? ThirdName { get; set; }

    public string LastName { get; set; } = null!;

    public DateOnly DateOfBirth { get; set; }

    public byte Gender { get; set; }

    public string? Phone { get; set; }

    public int NationalityCountryId { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Country NationalityCountry { get; set; } = null!;

    public virtual User? User { get; set; }
}
