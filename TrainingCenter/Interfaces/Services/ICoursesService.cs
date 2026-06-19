using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.DTOs.Courses;

namespace TrainingCenter.Core.Interfaces.Services
{
    public interface ICoursesService
    {
        Task<ApiResponse<string>> CreateCourseAsync(CreateCourseRequestDto request, int currentUserId);

        Task<ApiResponse<IEnumerable<CourseResponseDto>>> GetAllCoursesAsync(PagedFilterRequestDto request);

        Task<ApiResponse<CourseResponseDto>> GetCourseByIdAsync(int id);

        Task<ApiResponse<CourseResponseDto>> UpdateCourseAsync(int id, UpdateCourseRequestDto request, string userId, string userRole);

        Task<ApiResponse<string>> DeleteCourseAsync(int id, string adminId, string adminRole);

        Task<ApiResponse<IEnumerable<CourseResponseDto>>> GetCoursesByInstructorAsync(PagedFilterRequestDto request, int currentUserId);

        Task<ApiResponse<string>> ChangeCourseStatusAsync(int id, string newStatus);

        Task<ApiResponse<IEnumerable<CourseResponseDto>>> GetCoursesByInstructorIdAsync(PagedFilterRequestDto request, int instructorId);

        Task<ApiResponse<IEnumerable<CourseResponseDto>>> GetDeletedCoursesAsync(PagedFilterRequestDto request);

        Task<ApiResponse<string>> RestoreCourseAsync(int id, string adminId);
    }
}
