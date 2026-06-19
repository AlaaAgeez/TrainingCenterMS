using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingCenter.Core.DTOs.Enrollments
{
    public class UpdateEnrollmentDto
    {
        public decimal? ProgressPercent { get; set; }
        public decimal? FinalGrade { get; set; }
        public string? Status { get; set; }
    }
}
