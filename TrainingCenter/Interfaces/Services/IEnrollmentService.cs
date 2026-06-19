using System.Collections.Generic;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.DTOs.Enrollments;

namespace TrainingCenter.Core.Interfaces.Services
{
    public interface IEnrollmentService
    {
        Task<ApiResponse<string>> EnrollInCourseAsync(EnrollmentRequestDto request, int currentUserId);
        Task<ApiResponse<string>> UpdateEnrollmentAsync(int enrollmentId, UpdateEnrollmentDto request, int adminId);
        Task<ApiResponse<EnrollmentResponseDto>> GetEnrollmentByIdAsync(int enrollmentId, int currentUserId, bool isAdmin);
        Task<ApiResponse<IEnumerable<EnrollmentResponseDto>>> GetMyEnrollmentsAsync(int currentUserId);
        Task<ApiResponse<IEnumerable<EnrollmentResponseDto>>> GetAllEnrollmentsAsync(PagedFilterRequestDto request);
        Task<ApiResponse<string>> UnenrollAsync(int enrollmentId, int currentUserId, bool isAdmin);
        Task<ApiResponse<IEnumerable<EnrollmentResponseDto>>> GetEnrollmentsByCourseIdAsync(int courseId);
    }
}