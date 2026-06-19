using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingCenter.Core.DTOs.Courses
{
    public class CreateCourseRequestDto
    {
        public string Title { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string Level { get; set; } = string.Empty;

        public int DurationHours { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
