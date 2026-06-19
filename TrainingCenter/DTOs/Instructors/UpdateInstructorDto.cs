using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingCenter.Core.DTOs.Instructors
{
    public class UpdateInstructorDto
    {
        public string FirstName { get; set; } = null!;

        public string? SecondName { get; set; }

        public string? ThirdName { get; set; }

        public string LastName { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public decimal Salary { get; set; }
    }
}
