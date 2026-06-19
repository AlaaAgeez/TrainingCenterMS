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
using TrainingCenter.Core.DTOs.Enrollments;
using TrainingCenter.Core.Entities;
using TrainingCenter.Core.Exceptions;
using TrainingCenter.Core.Interfaces.Repositories;
using TrainingCenter.Core.Interfaces.Services;

namespace TrainingCenter.Business.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _cache;
        private readonly ILogger<EnrollmentService> _logger;

        public EnrollmentService(IUnitOfWork unitOfWork, IDistributedCache cache, ILogger<EnrollmentService> logger)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _logger = logger;
        }
        public async Task<ApiResponse<string>> EnrollInCourseAsync(EnrollmentRequestDto request, int currentUserId)
        {
            if (request == null || request.CourseId <= 0)
                throw new BadRequestException("Invalid Request.");

            var student = await _unitOfWork.Students.FindAsync(s => s.UserId == currentUserId && s.IsDeleted == false);
            if (student == null)
                throw new NotFoundException("Student profile not found.");

            var course = await _unitOfWork.Courses.FindAsync(c => c.CourseId == request.CourseId && c.IsDeleted == false);
            if (course == null)
                throw new NotFoundException("Course not found.");

            if (course.Status != "Published")
                throw new BadRequestException("You can only enroll in published courses.");

            var isAlreadyEnrolled = await _unitOfWork.Enrollments.AnyAsync(e => e.StudentId == student.StudentId && e.CourseId == request.CourseId && e.IsDeleted == false);
            if (isAlreadyEnrolled)
                throw new ConflictException("You are already enrolled in this course.");

            var enrollment = new Enrollment
            {
                StudentId = student.StudentId,
                CourseId = request.CourseId,
                EnrollmentDate = DateTime.UtcNow,
                ProgressPercent = 0, 
                Status = "Enrolled", 
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _unitOfWork.Enrollments.AddAsync(enrollment);

            try
            {
                await _unitOfWork.CompleteAsync();
                await _cache.RemoveAsync($"my_enrollments:{currentUserId}");
            }
            catch (Exception)
            {
                _logger.LogError("Database error occurred during enrollment for Student {StudentId}", student.StudentId);
                throw new BadRequestException("Failed to complete enrollment. Please try again.");
            }

            return new ApiResponse<string> { Success = true, Message = "Enrolled In Course Successfully" };
        }
        public async Task<ApiResponse<string>> UpdateEnrollmentAsync(int enrollmentId, UpdateEnrollmentDto request, int adminId)
        {
            if (enrollmentId <= 0 || request == null)
                throw new BadRequestException("Invalid Request.");

            var enrollment = await _unitOfWork.Enrollments.GetWithTrackingAsync(
                e => e.EnrollmentId == enrollmentId && e.IsDeleted == false, includes: ["Student"]);

            if (enrollment == null)
                throw new NotFoundException("Enrollment not found.");

            if (request.ProgressPercent.HasValue)
            {
                enrollment.ProgressPercent = request.ProgressPercent.Value;
                if (enrollment.ProgressPercent >= 100 && enrollment.Status != "Completed")
                {
                    enrollment.CompletionDate = DateTime.UtcNow;
                    enrollment.Status = "Completed";
                }
            }

            if (request.FinalGrade.HasValue)
                enrollment.FinalGrade = request.FinalGrade.Value;

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                enrollment.Status = request.Status.Trim();
                if (enrollment.Status == "Completed" && enrollment.CompletionDate == null)
                {
                    enrollment.CompletionDate = DateTime.UtcNow;
                }
            }

            enrollment.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Enrollments.Update(enrollment);

            await _unitOfWork.CompleteAsync();

            await _cache.RemoveAsync($"enrollment_details:{enrollmentId}");
            if (enrollment.Student != null)
            {
                await _cache.RemoveAsync($"my_enrollments:{enrollment.Student.UserId}");
            }

            return new ApiResponse<string> { Success = true, Message = "Enrollment Updated Successfully" };
        }

        public async Task<ApiResponse<EnrollmentResponseDto>> GetEnrollmentByIdAsync(int enrollmentId, int currentUserId, bool isAdmin)
        {
            if (enrollmentId <= 0)
                throw new BadRequestException("Invalid Request.");

            string cacheKey = $"enrollment_details:{enrollmentId}";
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                var cachedDto = JsonSerializer.Deserialize<EnrollmentResponseDto>(cachedData);
                if (cachedDto != null)
                {
                    if (!isAdmin && cachedDto.StudentId != currentUserId)
                        throw new ForbiddenException("Access Denied.");

                    return new ApiResponse<EnrollmentResponseDto> { Success = true, Message = "Success from Cache.", Data = cachedDto };
                }
            }

            var enrollment = await _unitOfWork.Enrollments.GetWithTrackingAsync(
                e => e.EnrollmentId == enrollmentId && e.IsDeleted == false,
                includes: ["Student", "Student.User", "Student.User.Person", "Course"]
            );

            if (enrollment == null)
                throw new NotFoundException("Enrollment not found.");

            if (!isAdmin && enrollment.Student?.UserId != currentUserId)
                throw new ForbiddenException("Access Denied.");

            var dto = new EnrollmentResponseDto
            {
                EnrollmentId = enrollment.EnrollmentId,
                StudentId = enrollment.StudentId,
                StudentName = enrollment.Student?.User?.Person != null ? $"{enrollment.Student.User.Person.FirstName} {enrollment.Student.User.Person.LastName}" : "Unknown",
                CourseId = enrollment.CourseId,
                CourseTitle = enrollment.Course?.Title ?? "Unknown Course",
                CourseCode = enrollment.Course?.Code ?? "",
                EnrollmentDate = enrollment.EnrollmentDate,
                CompletionDate = enrollment.CompletionDate,
                ProgressPercent = enrollment.ProgressPercent,
                FinalGrade = enrollment.FinalGrade,
                Status = enrollment.Status,
                CreatedAt = enrollment.CreatedAt
            };

            var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20) };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto), cacheOptions);

            return new ApiResponse<EnrollmentResponseDto> { Success = true, Message = "Enrollment Retrieved Successfully.", Data = dto };
        }
        public async Task<ApiResponse<IEnumerable<EnrollmentResponseDto>>> GetMyEnrollmentsAsync(int currentUserId)
        {
            if (currentUserId <= 0)
                throw new BadRequestException("Invalid Request.");

            string cacheKey = $"my_enrollments:{currentUserId}";
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                var cachedList = JsonSerializer.Deserialize<IEnumerable<EnrollmentResponseDto>>(cachedData);
                if (cachedList != null)
                    return new ApiResponse<IEnumerable<EnrollmentResponseDto>> { Success = true, Message = "Success from Cache.", Data = cachedList };
            }

            var student = await _unitOfWork.Students.FindAsync(s => s.UserId == currentUserId && s.IsDeleted == false);
            if (student == null)
                throw new NotFoundException("Student profile not found.");

            var enrollments = await _unitOfWork.Enrollments.FindAllAsync(
                match: e => e.StudentId == student.StudentId && e.IsDeleted == false,
                includes: ["Course", "Student.User.Person"]
            );

            var result = enrollments.OrderByDescending(e => e.EnrollmentDate).Select(e => new EnrollmentResponseDto
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                StudentName = $"{e.Student?.User?.Person?.FirstName} {e.Student?.User?.Person?.LastName}",
                CourseId = e.CourseId,
                CourseTitle = e.Course?.Title ?? "Unknown Course",
                CourseCode = e.Course?.Code ?? "",
                EnrollmentDate = e.EnrollmentDate,
                CompletionDate = e.CompletionDate,
                ProgressPercent = e.ProgressPercent,
                FinalGrade = e.FinalGrade,
                Status = e.Status,
                CreatedAt = e.CreatedAt
            }).ToList();

            var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), cacheOptions);

            return new ApiResponse<IEnumerable<EnrollmentResponseDto>> { Success = true, Message = "My Enrollments Retrieved Successfully.", Data = result };
        }

        public async Task<ApiResponse<IEnumerable<EnrollmentResponseDto>>> GetAllEnrollmentsAsync(PagedFilterRequestDto request)
        {
            Expression<Func<Enrollment, object>>? orderByExp = e => e.EnrollmentId;
            if (!string.IsNullOrEmpty(request.OrderBy))
            {
                orderByExp = request.OrderBy.ToLower() switch
                {
                    "enrollmentdate" => e => e.EnrollmentDate,
                    "progress" => e => e.ProgressPercent,
                    _ => e => e.EnrollmentId
                };
            }

            var direction = request.OrderByDirection?.ToLower() == "desc" ? "desc" : "asc";

            string search = request.SearchTerm?.Trim().ToLower() ?? "";

            var enrollments = await _unitOfWork.Enrollments.FindAllAsync(
                match: e => e.IsDeleted == false &&
                            (string.IsNullOrEmpty(search) ||
                             e.Status.ToLower().Contains(search) ||
                             e.Student.User.Person.FirstName.ToLower().Contains(search) ||
                             e.Course.Title.ToLower().Contains(search)),
                Take: request.Limit ?? 10,
                Skip: request.Page.HasValue ? (request.Page - 1) * request.Limit : 0,
                orderBy: orderByExp,
                orderByDirection: direction,
                includes: ["Student", "Student.User", "Student.User.Person", "Course"]
            );

            var result = enrollments.Select(e => new EnrollmentResponseDto
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                StudentName = e.Student?.User?.Person != null ? $"{e.Student.User.Person.FirstName} {e.Student.User.Person.LastName}" : "Unknown",
                CourseId = e.CourseId,
                CourseTitle = e.Course?.Title ?? "Unknown Course",
                CourseCode = e.Course?.Code ?? "",
                EnrollmentDate = e.EnrollmentDate,
                CompletionDate = e.CompletionDate,
                ProgressPercent = e.ProgressPercent,
                FinalGrade = e.FinalGrade,
                Status = e.Status,
                CreatedAt = e.CreatedAt
            }).ToList();

            return new ApiResponse<IEnumerable<EnrollmentResponseDto>> { Success = true, Message = "Enrollments Retrieved Successfully.", Data = result };
        }

        public async Task<ApiResponse<string>> UnenrollAsync(int enrollmentId, int currentUserId, bool isAdmin)
        {
            if (enrollmentId <= 0)
                throw new BadRequestException("Invalid Request.");

            var enrollment = await _unitOfWork.Enrollments.GetWithTrackingAsync(
                e => e.EnrollmentId == enrollmentId && e.IsDeleted == false,
                includes: ["Student"]
            );

            if (enrollment == null)
                throw new NotFoundException("Enrollment not found.");

            if (!isAdmin && enrollment.Student?.UserId != currentUserId)
                throw new ForbiddenException("Access Denied. You can only drop your own enrollments.");

            enrollment.IsDeleted = true;
            enrollment.UpdatedAt = DateTime.UtcNow;
            enrollment.Status = "Dropped";

            _unitOfWork.Enrollments.Update(enrollment);
            await _unitOfWork.CompleteAsync();

            await _cache.RemoveAsync($"enrollment_details:{enrollmentId}");
            if (enrollment.Student != null)
            {
                await _cache.RemoveAsync($"my_enrollments:{enrollment.Student.UserId}");
                await _cache.RemoveAsync($"student_profile:{enrollment.Student.UserId}");
            }

            _logger.LogInformation("Enrollment ID {EnrollmentId} was soft-deleted by User ID {CurrentUserId}.", enrollmentId, currentUserId);

            return new ApiResponse<string> { Success = true, Message = "Unenrolled from course successfully." };
        }

        public async Task<ApiResponse<IEnumerable<EnrollmentResponseDto>>> GetEnrollmentsByCourseIdAsync(int courseId)
        {
            if (courseId <= 0)
                throw new BadRequestException("Invalid Course ID.");

            var course = await _unitOfWork.Courses.FindAsync(c => c.CourseId == courseId && c.IsDeleted == false);
            if (course == null)
                throw new NotFoundException("Course not found.");

            var enrollments = await _unitOfWork.Enrollments.FindAllAsync(
                match: e => e.CourseId == courseId && e.IsDeleted == false,
                includes: ["Student", "Student.User", "Student.User.Person", "Course"]
            );

            var result = enrollments.Select(e => new EnrollmentResponseDto
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                StudentName = e.Student?.User?.Person != null
                    ? $"{e.Student.User.Person.FirstName} {e.Student.User.Person.LastName}"
                    : "Unknown",
                CourseId = e.CourseId,
                CourseTitle = course.Title,
                CourseCode = course.Code,
                EnrollmentDate = e.EnrollmentDate,
                CompletionDate = e.CompletionDate,
                ProgressPercent = e.ProgressPercent,
                FinalGrade = e.FinalGrade,
                Status = e.Status,
                CreatedAt = e.CreatedAt
            }).ToList();

            return new ApiResponse<IEnumerable<EnrollmentResponseDto>>
            {
                Success = true,
                Message = $"Enrollments for course ID {courseId} retrieved successfully.",
                Data = result
            };
        }
    }
}