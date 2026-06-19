using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.DTOs.Students;
using TrainingCenter.Core.Entities;
using TrainingCenter.Core.Exceptions;
using TrainingCenter.Core.Interfaces.Repositories;
using TrainingCenter.Core.Interfaces.Services;
using TrainingCenter.DataAccess.Repositories;

namespace TrainingCenter.Business.Services
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _cache;
        private readonly ILogger<StudentService> _logger;

        public StudentService(IUnitOfWork unitOfWork, IDistributedCache cache, ILogger<StudentService> logger)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<StudentResponseDto>>> GetAllStudentsAsync(PagedFilterRequestDto request)
        {
            Expression<Func<Student, object>>? orderByExp = null;
            if (request.OrderBy != null)
            {
                orderByExp = request.OrderBy.ToLower() switch
                {
                    "studentid" => s => s.StudentId,
                    "registeredat" => s => s.CreatedAt,
                    "createdat" => s => s.CreatedAt,
                    _ => null
                };
            }

            var direction = request.OrderByDirection?.ToLower().StartsWith("desc") == true ? "desc" : "asc";

            Expression<Func<Student, bool>> filter = s => s.IsDeleted == false;

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim().ToLower();
                filter = s => s.IsDeleted == false && s.Status != null && s.Status.ToLower().Contains(search);
            }

            var students = await _unitOfWork.Students.FindAllStudentsWithUsersAsync(
                request: request,
                match: filter,
                orderBy: orderByExp,
                orderByDirection: direction
            );

            var result = students.Select(s => new StudentResponseDto
            {
                StudentId = s.StudentId,
                CreatedAt = s.CreatedAt,
                Status = s.Status ?? "Unknown",
                UserId = s.UserId,
                StudentName = s.User?.Person != null ? $"{s.User.Person.FirstName} {s.User.Person.LastName}" : "Unknown Student",
                Email = s.User?.Email ?? "No Email",
            }).ToList();

            return new ApiResponse<IEnumerable<StudentResponseDto>> { Success = true, Message = "Students retrieved successfully.", Data = result };
        }

        public async Task<ApiResponse<StudentResponseDto>> GetStudentProfileAsync(int currentUserId)
        {
            if (currentUserId <= 0)
                throw new BadRequestException("Invalid Request.");

            string cacheKey = $"student_profile:{currentUserId}";

            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                var cachedDto = JsonSerializer.Deserialize<StudentResponseDto>(cachedData);

                if (cachedDto != null)
                    return new ApiResponse<StudentResponseDto> { Success = true, Message = "Success from Cache.", Data = cachedDto };
            }

            var student = await _unitOfWork.Students.GetStudentByUserIdAsync(currentUserId);

            if (student == null)
                throw new NotFoundException("Student profile not found.");

            var studentDto = new StudentResponseDto
            {
                StudentId = student.StudentId,
                CreatedAt = student.CreatedAt,
                Status = student.Status,
                UserId = student.UserId,
                StudentName = student.User?.Person != null
                    ? $"{student.User.Person.FirstName} {student.User.Person.LastName}"
                    : "Unknown Student",
                Email = student.User?.Email ?? "No Email",
            };

            var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) };

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(studentDto), cacheOptions);

            return new ApiResponse<StudentResponseDto> { Success = true, Message = "Student profile retrieved successfully.", Data = studentDto };
        }

        public async Task<ApiResponse<string>> ChangeStatusAsync(int studentId, string newStatus, int adminId)
        {
            var validStatuses = new[] { "Active", "Inactive", "Suspended" };
            if (studentId <= 0 || !validStatuses.Contains(newStatus))
                throw new BadRequestException("Invalid status value.");

            var student = await _unitOfWork.Students.FindAsync(s => s.StudentId == studentId && s.IsDeleted == false);

            if (student == null)
                throw new NotFoundException("Student not found.");

            var oldStatus = student.Status;
            student.Status = newStatus.Trim();

            _unitOfWork.Students.Update(student);

            try
            {
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Admin ID {AdminId} successfully updated Student ID {StudentId} status from '{OldStatus}' to '{NewStatus}'.",
                    adminId, studentId, oldStatus, newStatus);

                await _cache.RemoveAsync($"student_profile:{student.UserId}");
                await _cache.RemoveAsync($"student_details:{student.StudentId}");

                _logger.LogInformation("Cache invalidated for student ID {StudentId} and User ID {UserId}.", student.StudentId, student.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while Admin {AdminId} was updating status for student ID {StudentId} to '{NewStatus}'.",
                    adminId, studentId, newStatus);
                throw;
            }

            return new ApiResponse<string> { Success = true, Message = $"Student status updated to '{newStatus}' successfully.", Data = newStatus };
        }

        public async Task<ApiResponse<string>> DeleteStudentAsync(int studentId, int adminId)
        {
            if (studentId <= 0)
                throw new BadRequestException("Invalid student ID.");

            var student = await _unitOfWork.Students.FindAsync(s => s.StudentId == studentId && s.IsDeleted == false);

            if (student == null)
                throw new NotFoundException("Student not found or already deleted.");

            student.IsDeleted = true;

            try
            {
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Admin with ID {AdminId} successfully soft-deleted student with ID {StudentId}.", adminId, studentId);

                await _cache.RemoveAsync($"student_profile:{student.UserId}");
                await _cache.RemoveAsync($"student_details:{student.StudentId}");

                _logger.LogInformation("Cache invalidated after student deletion.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while Admin {AdminId} was soft-deleting student {StudentId}.", adminId, studentId);
                throw;
            }

            return new ApiResponse<string> { Success = true, Message = "Student deleted successfully (Soft Delete).", Data = $"Student ID: {studentId}" };
        }

        public async Task<ApiResponse<StudentResponseDto>> GetStudentByIdAsync(int studentId)
        {
            if (studentId <= 0)
                throw new BadRequestException("Invalid Request.");

            string cacheKey = $"student_details:{studentId}";

            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                var cachedDto = JsonSerializer.Deserialize<StudentResponseDto>(cachedData);
                if (cachedDto != null)
                    return new ApiResponse<StudentResponseDto> { Success = true, Message = "Success from Cache.", Data = cachedDto };
            }

            var student = await _unitOfWork.Students.GetWithTrackingAsync(
                s => s.StudentId == studentId && s.IsDeleted == false,
                includes: new[] { "User", "User.Person" }
            );

            if (student == null)
                throw new NotFoundException("Student not found.");

            var studentDto = new StudentResponseDto
            {
                StudentId = student.StudentId,
                CreatedAt = student.CreatedAt,
                Status = student.Status,
                UserId = student.UserId,
                StudentName = student.User?.Person != null
                    ? $"{student.User.Person.FirstName} {student.User.Person.LastName}"
                    : "Unknown Student",
                Email = student.User?.Email ?? "No Email",
            };

            var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20) };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(studentDto), cacheOptions);

            return new ApiResponse<StudentResponseDto> { Success = true, Message = "Student Retrieved Successfully.", Data = studentDto };
        }

        public async Task<ApiResponse<string>> UpdateStudentAsync(int studentId, UpdateStudentDto updateDto, int currentUserId)
        {
            if (studentId <= 0 || updateDto == null)
                throw new BadRequestException("Invalid Request.");

            var student = await _unitOfWork.Students.FindAsync(s => s.StudentId == studentId && s.IsDeleted == false);
            if (student == null)
                throw new NotFoundException("Student not found.");

            if (!string.IsNullOrWhiteSpace(updateDto.Status))
            {
                var validStatuses = new[] { "Active", "Inactive", "Suspended" };
                if (!validStatuses.Contains(updateDto.Status))
                    throw new BadRequestException("Invalid Status.");

                student.Status = updateDto.Status.Trim();
            }

            _unitOfWork.Students.Update(student);

            try
            {
                await _unitOfWork.CompleteAsync();

                await _cache.RemoveAsync($"student_profile:{student.UserId}");
                await _cache.RemoveAsync($"student_details:{student.StudentId}");

                _logger.LogInformation("Student ID {StudentId} successfully updated by User ID {CurrentUserId}.", studentId, currentUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating student ID {StudentId} by User ID {CurrentUserId}.", studentId, currentUserId);
                throw;
            }

            return new ApiResponse<string> { Success = true, Message = "Student Updated Successfully" };
        }
    }
}