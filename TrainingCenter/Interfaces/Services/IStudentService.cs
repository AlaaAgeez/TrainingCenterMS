using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.DTOs.Students;

namespace TrainingCenter.Core.Interfaces.Services
{
    public interface IStudentService
    {
        Task<ApiResponse<IEnumerable<StudentResponseDto>>> GetAllStudentsAsync(PagedFilterRequestDto request);

        Task<ApiResponse<StudentResponseDto>> GetStudentProfileAsync(int currentUserId);

        Task<ApiResponse<string>> ChangeStatusAsync(int studentId, string newStatus, int adminId);

        Task<ApiResponse<string>> DeleteStudentAsync(int studentId, int adminId);

        Task<ApiResponse<StudentResponseDto>> GetStudentByIdAsync(int studentId);

        Task<ApiResponse<string>> UpdateStudentAsync(int studentId, UpdateStudentDto updateDto, int currentUserId);
    }
}
