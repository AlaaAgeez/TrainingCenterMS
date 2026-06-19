using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.DTOs.Instructors;

namespace TrainingCenter.Core.Interfaces.Services
{
    public interface IInstructorsService
    {
        Task<ApiResponse<string>> CreateInstructor(CreateInstructorRequestDto request, int CruuntAdmin);

        Task<ApiResponse<IEnumerable<InstructorResponseDto>>> GetAllInstructorsAsync(PagedFilterRequestDto request);

        Task<ApiResponse<InstructorResponseDto>> GetInstructorByIdAsync(int instructorId);

        Task<ApiResponse<InstructorResponseDto>> GetInstructorByUserIdAsync(int currentUserId);

        Task<ApiResponse<string>> UpdateInstructorAsync(int instructorId, UpdateInstructorDto request, int adminUserId);

        Task<ApiResponse<string>> DeleteInstructorAsync(int instructorId, int adminUserId);
    }
}
