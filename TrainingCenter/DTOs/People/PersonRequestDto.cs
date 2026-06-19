using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingCenter.Core.DTOs.People
{
    public class PersonRequestDto
    {
        public string NationalNo { get; set; }
        public string FirstName { get; set; }
        public string? SecondName { get; set; }
        public string? ThirdName { get; set; }
        public string LastName { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public byte Gender { get; set; }
        public string? Phone { get; set; }
        public int NationalityCountryId { get; set; }
    }
}
