using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.DTOs.Instructors;
using TrainingCenter.Core.Entities;

namespace TrainingCenter.Core.Interfaces.Repositories
{
    public interface IInstructorRepository : IBaseRepository<Instructor>
    {
        Task<IEnumerable<InstructorResponseDto>> GetPagedInstructorsAsync(PagedFilterRequestDto request);

        Task<InstructorResponseDto?> GetInstructorByIdAsync(int instructorId);
    }
}
