using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.DTOs.Courses;
using TrainingCenter.Core.Entities;
using TrainingCenter.Core.Exceptions;
using TrainingCenter.Core.Interfaces.Repositories;
using TrainingCenter.Core.Interfaces.Services;
using TrainingCenter.DataAccess.Repositories;

namespace TrainingCenter.Business.Services
{
    public class CoursesService : ICoursesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CoursesService> _logger;
        private readonly IDistributedCache _cache;

        public CoursesService(IUnitOfWork unitOfWork, ILogger<CoursesService> logger, IDistributedCache cache)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cache = cache;
        }

        public async Task<ApiResponse<string>> CreateCourseAsync(CreateCourseRequestDto request, int currentUserId)
        {
            if (request == null || currentUserId <= 0)
                throw new BadRequestException("Invalid Request.");

            var user = await _unitOfWork.Users.GetByIdAsync(currentUserId);
            if (user == null || user.IsDeleted == true)
                throw new NotFoundException("User account not found.");
            if (!user.IsActive)
                throw new BadRequestException("Account is deactivated.");

            var instructor = await _unitOfWork.Instructors.FindAsync(i => i.UserId == currentUserId && !i.IsDeleted);
            if (instructor == null)
                throw new NotFoundException("This user is not registered as an active instructor.");

            if (await _unitOfWork.Courses.AnyAsync(c => c.Code.ToLower() == request.Code.Trim().ToLower() && !c.IsDeleted))
                throw new BadRequestException($"Course code '{request.Code}' already exists.");

            if (await _unitOfWork.Courses.AnyAsync(c => c.Title.ToLower() == request.Title.Trim().ToLower() && !c.IsDeleted))
                throw new BadRequestException($"Course title '{request.Title}' already exists.");

            var course = new Course
            {
                Title = request.Title.Trim(),
                Code = request.Code.Trim().ToUpper(),
                Description = request.Description?.Trim(),
                Price = request.Price,
                Level = request.Level.Trim(),
                DurationHours = request.DurationHours,
                InstructorId = instructor.InstructorId,
                Status = "Draft",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            try
            {
                await _unitOfWork.Courses.AddAsync(course);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException?.Message ?? ex.Message;
                _logger.LogError("Database Error: {Message}", innerMsg);
                throw new BadRequestException($"Database error: {innerMsg}");
            }

            await _cache.RemoveAsync("courses:all");
            _logger.LogInformation("Instructor {UserId} created course {Code}", currentUserId, course.Code);

            return new ApiResponse<string> { Success = true, Message = "Course created successfully.", Data = course.CourseId.ToString() };
        }

        public async Task<ApiResponse<IEnumerable<CourseResponseDto>>> GetAllCoursesAsync(PagedFilterRequestDto request)
        {
            Expression<Func<Course, object>>? orderByExp = null;

            if (request.OrderBy != null)
            {
                orderByExp = request.OrderBy.ToLower() switch
                {
                    "courseid" => c => c.CourseId,
                    "title" => c => c.Title,
                    "code" => c => c.Code,
                    "price" => c => c.Price,
                    "durationhours" => c => c.DurationHours,
                    "createdat" => c => c.CreatedAt,
                    _ => null
                };
            }

            var direction = request.OrderByDirection?.ToLower().StartsWith("desc") == true ? "desc" : "asc";

            Expression<Func<Course, bool>> filter = c =>
                c.IsDeleted == false &&
                c.Instructor != null &&
                c.Instructor.IsDeleted == false &&
                c.Instructor.User != null &&
                c.Instructor.User.IsActive == true;

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim().ToLower();

                var existingFilter = filter; 
                filter = c => existingFilter.Compile()(c) &&
                             (c.Title.ToLower().Contains(search) || c.Code.ToLower().Contains(search));
            }

            var courses = await _unitOfWork.Courses.FindAllCoursesWithInstructorAsync(
                request: request,
                match: filter,
                orderBy: orderByExp,
                orderByDirection: direction
            );

            var result = courses.Select(c => new CourseResponseDto
            {
                CourseId = c.CourseId,
                Title = c.Title,
                Code = c.Code,
                Description = c.Description,
                Price = c.Price,
                Level = c.Level,
                DurationHours = c.DurationHours,
                Status = c.Status,
                InstructorId = c.InstructorId,
                InstructorName = c.Instructor?.User?.Person != null
                    ? $"{c.Instructor.User.Person.FirstName} {c.Instructor.User.Person.LastName}"
                    : "Unknown Instructor",
                CreatedAt = c.CreatedAt,
                PublishedAt = c.PublishedAt
            }).ToList();

            return new ApiResponse<IEnumerable<CourseResponseDto>> { Success = true, Message = "Data retrieved successfully.", Data = result };
        }

        public async Task<ApiResponse<CourseResponseDto>> GetCourseByIdAsync(int id)
        {
            if (id <= 0)
                throw new BadRequestException("Invalid Request.");

            string cacheKey = $"course:{id}";

            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                var cachedDto = JsonSerializer.Deserialize<CourseResponseDto>(cachedData);
                return new ApiResponse<CourseResponseDto> { Success = true, Message = "Success from Cache.", Data = cachedDto };
            }

            var course = await _unitOfWork.Courses.FindAsync(
                match: c => c.CourseId == id &&
                            c.IsDeleted == false &&
                            c.Instructor != null &&
                            c.Instructor.IsDeleted == false &&
                            c.Instructor.User != null &&
                            c.Instructor.User.IsActive == true,
                includes: new string[] { "Instructor.User.Person" }
            );

            if (course == null)
                throw new NotFoundException($"Course with ID {id} not found or instructor is inactive.");

            var result = new CourseResponseDto
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Code = course.Code,
                Description = course.Description,
                Price = course.Price,
                Level = course.Level,
                DurationHours = course.DurationHours,
                Status = course.Status,
                InstructorId = course.InstructorId,
                InstructorName = course.Instructor?.User?.Person != null
                    ? $"{course.Instructor.User.Person.FirstName} {course.Instructor.User.Person.LastName}"
                    : "Unknown Instructor",
                CreatedAt = course.CreatedAt,
                PublishedAt = course.PublishedAt
            };

            var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1) };
            var serializedData = JsonSerializer.Serialize(result);
            await _cache.SetStringAsync(cacheKey, serializedData, cacheOptions);

            return new ApiResponse<CourseResponseDto> { Success = true, Message = "Course retrieved successfully.", Data = result };
        }

        public async Task<ApiResponse<CourseResponseDto>> UpdateCourseAsync(int id, UpdateCourseRequestDto request, string userId, string userRole)
        {
            if (id <= 0 || request == null)
                throw new BadRequestException("Invalid Request.");

            var course = await _unitOfWork.Courses.FindAsync(
                match: c => c.CourseId == id && c.IsDeleted == false,
                includes: new string[] { "Instructor.User.Person" }
            );

            if (course == null)
                throw new NotFoundException($"Course with ID {id} not found.");

            if (!string.IsNullOrWhiteSpace(request.Title))
                course.Title = request.Title;

            if (!string.IsNullOrWhiteSpace(request.Description))
                course.Description = request.Description;

            if (request.Price.HasValue)
                course.Price = request.Price.Value;

            if (!string.IsNullOrWhiteSpace(request.Level))
                course.Level = request.Level;

            if (request.DurationHours.HasValue)
                course.DurationHours = request.DurationHours.Value;

            if (!string.IsNullOrWhiteSpace(request.Status))
                course.Status = request.Status;

            if (request.InstructorId.HasValue)
                course.InstructorId = request.InstructorId.Value;

            _unitOfWork.Courses.Update(course);
            await _unitOfWork.CompleteAsync();

            string cacheKey = $"course:{id}";
            await _cache.RemoveAsync(cacheKey);

            _logger.LogInformation("Course with ID {CourseId} was successfully updated by User {UserId} with Role ({UserRole}).",
                id, userId, userRole);

            var result = new CourseResponseDto
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Code = course.Code,
                Description = course.Description,
                Price = course.Price,
                Level = course.Level,
                DurationHours = course.DurationHours,
                Status = course.Status,
                InstructorId = course.InstructorId,
                InstructorName = course.Instructor?.User?.Person != null
                    ? $"{course.Instructor.User.Person.FirstName} {course.Instructor.User.Person.LastName}"
                    : "Unknown Instructor",
                CreatedAt = course.CreatedAt,
                PublishedAt = course.PublishedAt
            };

            return new ApiResponse<CourseResponseDto> { Success = true, Message = "Course updated successfully.", Data = result };
        }

        public async Task<ApiResponse<string>> DeleteCourseAsync(int id, string adminId, string adminRole)
        {
            if (id <= 0)
                throw new BadRequestException("Invalid Request.");

            var course = await _unitOfWork.Courses.FindAsync(c => c.CourseId == id && c.IsDeleted == false);

            if (course == null)
                throw new NotFoundException($"Course with ID {id} not found or already deleted.");

            course.IsDeleted = true;

            _unitOfWork.Courses.Update(course);
            await _unitOfWork.CompleteAsync();

            string cacheKey = $"course:{id}";
            await _cache.RemoveAsync(cacheKey);

            _logger.LogInformation("Course with ID {CourseId} was Soft-Deleted by Admin {AdminId} with Role ({AdminRole}).",
                id, adminId, adminRole);

            return new ApiResponse<string> { Success = true, Message = "Course deleted successfully.", Data = $"Course ID {id} has been soft deleted." };
        }

        public async Task<ApiResponse<IEnumerable<CourseResponseDto>>> GetCoursesByInstructorAsync(PagedFilterRequestDto request, int currentUserId)
        {
            var instructor = await _unitOfWork.Instructors.FindAsync(
                match: i => i.UserId == currentUserId && i.IsDeleted == false
            );

            if (instructor == null)
                throw new NotFoundException("Instructor account not found.");

            Expression<Func<Course, object>>? orderByExp = null;
            if (request.OrderBy != null)
            {
                orderByExp = request.OrderBy.ToLower() switch
                {
                    "courseid" => c => c.CourseId,
                    "title" => c => c.Title,
                    "code" => c => c.Code,
                    "price" => c => c.Price,
                    "durationhours" => c => c.DurationHours,
                    "createdat" => c => c.CreatedAt,
                    _ => null
                };
            }

            var direction = request.OrderByDirection?.ToLower().StartsWith("desc") == true ? "desc" : "asc";

            Expression<Func<Course, bool>> filter = c =>
                c.IsDeleted == false &&
                c.InstructorId == instructor.InstructorId;

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim().ToLower();
                filter = c =>
                    c.IsDeleted == false &&
                    c.InstructorId == instructor.InstructorId &&
                    (c.Title.ToLower().Contains(search) || c.Code.ToLower().Contains(search));
            }

            var courses = await _unitOfWork.Courses.FindAllCoursesWithInstructorAsync(
                request: request,
                match: filter,
                orderBy: orderByExp,
                orderByDirection: direction
            );

            var result = courses.Select(c => new CourseResponseDto
            {
                CourseId = c.CourseId,
                Title = c.Title,
                Code = c.Code,
                Description = c.Description,
                Price = c.Price,
                Level = c.Level,
                DurationHours = c.DurationHours,
                Status = c.Status,
                InstructorId = c.InstructorId,
                InstructorName = c.Instructor?.User?.Person != null
                    ? $"{c.Instructor.User.Person.FirstName} {c.Instructor.User.Person.LastName}"
                    : "Unknown Instructor",
                CreatedAt = c.CreatedAt,
                PublishedAt = c.PublishedAt
            }).ToList();

            return new ApiResponse<IEnumerable<CourseResponseDto>>
            {
                Success = true,
                Message = "Your courses retrieved successfully.",
                Data = result
            };
        }

        public async Task<ApiResponse<string>> ChangeCourseStatusAsync(int id, string newStatus)
        {
            if (id <= 0 || string.IsNullOrWhiteSpace(newStatus))
                throw new BadRequestException("Invalid Request Data.");

            var course = await _unitOfWork.Courses.FindAsync(c => c.CourseId == id && c.IsDeleted == false);

            if (course == null)
                throw new NotFoundException($"Course with ID {id} not found.");

            var validStatuses = new[] { "Draft", "Published", "Archived" };
            if (!validStatuses.Contains(newStatus, StringComparer.OrdinalIgnoreCase))
                throw new BadRequestException($"Invalid status. Allowed values are: {string.Join(", ", validStatuses)}");

            course.Status = newStatus;

            if (newStatus.Equals("Published", StringComparison.OrdinalIgnoreCase) && course.PublishedAt == null)
            {
                course.PublishedAt = DateTime.UtcNow;
            }

            _unitOfWork.Courses.Update(course);
            await _unitOfWork.CompleteAsync();

            string cacheKey = $"course:{id}";
            await _cache.RemoveAsync(cacheKey);

            _logger.LogInformation("Status of Course ID {CourseId} was updated to '{NewStatus}'.", id, newStatus);

            return new ApiResponse<string> { Success = true, Message = "Course status updated successfully.", Data = $"Course ID {id} status changed to {newStatus}." };
        }

        public async Task<ApiResponse<IEnumerable<CourseResponseDto>>> GetCoursesByInstructorIdAsync(PagedFilterRequestDto request, int instructorId)
        {
            if (instructorId <= 0)
                throw new BadRequestException("Invalid Instructor ID.");

            var instructorExists = await _unitOfWork.Instructors.FindAsync(i => i.InstructorId == instructorId && i.IsDeleted == false);
            if (instructorExists == null)
                throw new NotFoundException($"Instructor with ID {instructorId} not found.");

            Expression<Func<Course, object>>? orderByExp = null;
            if (request.OrderBy != null)
            {
                orderByExp = request.OrderBy.ToLower() switch
                {
                    "courseid" => c => c.CourseId,
                    "title" => c => c.Title,
                    "price" => c => c.Price,
                    "durationhours" => c => c.DurationHours,
                    "createdat" => c => c.CreatedAt,
                    _ => null
                };
            }

            var direction = request.OrderByDirection?.ToLower().StartsWith("desc") == true ? "desc" : "asc";

            Expression<Func<Course, bool>> filter = c =>
                c.InstructorId == instructorId &&
                c.IsDeleted == false &&
                c.Status == "Published";

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim().ToLower();
                filter = c =>
                    c.InstructorId == instructorId &&
                    c.IsDeleted == false &&
                    c.Status == "Published" &&
                    (c.Title.ToLower().Contains(search) || c.Code.ToLower().Contains(search));
            }

            var courses = await _unitOfWork.Courses.FindAllCoursesWithInstructorAsync(
                request: request,
                match: filter,
                orderBy: orderByExp,
                orderByDirection: direction
            );

            var result = courses.Select(c => new CourseResponseDto
            {
                CourseId = c.CourseId,
                Title = c.Title,
                Code = c.Code,
                Description = c.Description,
                Price = c.Price,
                Level = c.Level,
                DurationHours = c.DurationHours,
                Status = c.Status,
                InstructorId = c.InstructorId,
                InstructorName = c.Instructor?.User?.Person != null
                    ? $"{c.Instructor.User.Person.FirstName} {c.Instructor.User.Person.LastName}"
                    : "Unknown Instructor",
                CreatedAt = c.CreatedAt,
                PublishedAt = c.PublishedAt
            }).ToList();

            return new ApiResponse<IEnumerable<CourseResponseDto>> { Success = true, Message = "Instructor courses retrieved successfully.", Data = result };
        }

        public async Task<ApiResponse<IEnumerable<CourseResponseDto>>> GetDeletedCoursesAsync(PagedFilterRequestDto request)
        {
            Expression<Func<Course, object>>? orderByExp = null;
            if (request.OrderBy != null)
            {
                orderByExp = request.OrderBy.ToLower() switch
                {
                    "courseid" => c => c.CourseId,
                    "title" => c => c.Title,
                    "price" => c => c.Price,
                    "durationhours" => c => c.DurationHours,
                    "createdat" => c => c.CreatedAt,
                    _ => null
                };
            }

            var direction = request.OrderByDirection?.ToLower().StartsWith("desc") == true ? "desc" : "asc";

            Expression<Func<Course, bool>> filter = c => c.IsDeleted == true;

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim().ToLower();
                filter = c =>
                    c.IsDeleted == true &&
                    (c.Title.ToLower().Contains(search) || c.Code.ToLower().Contains(search));
            }

            var deletedCourses = await _unitOfWork.Courses.FindAllCoursesWithInstructorAsync(
                request: request,
                match: filter,
                orderBy: orderByExp,
                orderByDirection: direction
            );

            var result = deletedCourses.Select(c => new CourseResponseDto
            {
                CourseId = c.CourseId,
                Title = c.Title,
                Code = c.Code,
                Description = c.Description,
                Price = c.Price,
                Level = c.Level,
                DurationHours = c.DurationHours,
                Status = c.Status,
                InstructorId = c.InstructorId,
                InstructorName = c.Instructor?.User?.Person != null
                    ? $"{c.Instructor.User.Person.FirstName} {c.Instructor.User.Person.LastName}"
                    : "Unknown Instructor",
                CreatedAt = c.CreatedAt,
                PublishedAt = c.PublishedAt
            }).ToList();

            return new ApiResponse<IEnumerable<CourseResponseDto>> { Success = true, Message = "Soft-deleted courses retrieved successfully.", Data = result };
        }

        public async Task<ApiResponse<string>> RestoreCourseAsync(int id, string adminId)
        {
            if (id <= 0)
                throw new BadRequestException("Invalid Course ID.");

            var course = await _unitOfWork.Courses.FindAsync(c => c.CourseId == id && c.IsDeleted == true);

            if (course == null)
                throw new NotFoundException($"Course with ID {id} was not found in deleted courses.");

            course.IsDeleted = false;
            course.Status = "Draft";

            _unitOfWork.Courses.Update(course);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Course with ID {CourseId} has been successfully restored by Admin (User ID: {AdminId}).", id, adminId);

            return new ApiResponse<string> { Success = true, Message = "Course restored successfully.", Data = $"Course ID {id} is now active again as a Draft." };
        }
    }
}
