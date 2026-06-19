using System;
using System.Collections.Generic;

namespace TrainingCenter.Core.Entities;

public partial class Instructor
{
    public int InstructorId { get; set; }

    public DateOnly HireDate { get; set; }

    public decimal Salary { get; set; }

    public int? ManagerId { get; set; }

    public bool IsDeleted { get; set; }

    public int UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<Instructor> InverseManager { get; set; } = new List<Instructor>();

    public virtual Instructor? Manager { get; set; }

    public virtual User User { get; set; } = null!;
}
