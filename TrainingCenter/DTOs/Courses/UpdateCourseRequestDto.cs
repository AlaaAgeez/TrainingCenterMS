using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingCenter.Core.DTOs.Courses
{
    public class UpdateCourseRequestDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? Level { get; set; }
        public int? DurationHours { get; set; }
        public string? Status { get; set; }
        public int? InstructorId { get; set; }
    }
}
