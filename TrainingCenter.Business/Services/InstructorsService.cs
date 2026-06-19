using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.DTOs.Countries;
using TrainingCenter.Core.DTOs.Instructors;
using TrainingCenter.Core.DTOs.Roles;
using TrainingCenter.Core.Entities;
using TrainingCenter.Core.Exceptions;
using TrainingCenter.Core.Interfaces.Repositories;
using TrainingCenter.Core.Interfaces.Services;
using TrainingCenter.DataAccess.Repositories;

namespace TrainingCenter.Business.Services
{
    public class InstructorsService : IInstructorsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<InstructorsService> _logger;
        private readonly IDistributedCache _cache;

        public InstructorsService(IUnitOfWork unitOfWork, ILogger<InstructorsService> logger, IDistributedCache cache)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cache = cache;
        }
        public async Task<ApiResponse<string>> CreateInstructor(CreateInstructorRequestDto request, int adminUserId)
        {
            if (request == null || adminUserId <= 0)
                throw new BadRequestException("Invalid Request.");

            var userExists = await _unitOfWork.Users.AnyAsync(u => u.UserId == request.UserId && u.IsDeleted == false);
            if (!userExists)
                throw new NotFoundException("User not found.");

            var isInstructorExists = await _unitOfWork.Instructors.AnyAsync(x => x.UserId == request.UserId);
            if (isInstructorExists)
                throw new BadRequestException("Instructor already exists for this user.");

            var instructor = new Instructor
            {
                HireDate = request.HireDate,
                Salary = request.Salary,
                ManagerId = request.ManagerId,
                IsDeleted = false,
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Instructors.AddAsync(instructor);
            if (await _unitOfWork.CompleteAsync() <= 0)
                throw new BadRequestException("Failed to create instructor.");

            await _cache.RemoveAsync("all_instructors");

            _logger.LogInformation("Instructor created with ID: {InstructorId} by Admin ID: {AdminId}.", instructor.InstructorId, adminUserId);

            return new ApiResponse<string> { Success = true, Message = "Instructor created successfully." };
        }

        public async Task<ApiResponse<IEnumerable<InstructorResponseDto>>> GetAllInstructorsAsync(PagedFilterRequestDto request)
        {
            var instructors = await _unitOfWork.Instructors.GetPagedInstructorsAsync(request);

            if (instructors == null || !instructors.Any())
            {
                return new ApiResponse<IEnumerable<InstructorResponseDto>>
                {
                    Success = true,
                    Data = Enumerable.Empty<InstructorResponseDto>(),
                    Message = "No instructors found matching the criteria."
                };
            }

            return new ApiResponse<IEnumerable<InstructorResponseDto>>
            {
                Success = true,
                Message = "Instructors retrieved successfully.",
                Data = instructors
            };
        }
        public async Task<ApiResponse<InstructorResponseDto>> GetInstructorByIdAsync(int instructorId)
        {
            if (instructorId <= 0)
                throw new BadRequestException("Invalid Request.");

            string cacheKey = $"instructor:{instructorId}";

            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                var cachedDto = JsonSerializer.Deserialize<InstructorResponseDto>(cachedData);
                if (cachedDto != null)
                    return new ApiResponse<InstructorResponseDto> { Success = true, Message = "Success from Cache.", Data = cachedDto };
            }

            var instructorDto = await _unitOfWork.Instructors.GetInstructorByIdAsync(instructorId);

            if (instructorDto == null)
                throw new NotFoundException("Instructor not found.");

            if (!instructorDto.IsEmailVerified)
                throw new BadRequestException("This instructor account is unVerified.");

            if (!instructorDto.IsActive)
                throw new BadRequestException("This instructor account is deactivated.");

            var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1) };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(instructorDto), cacheOptions);

            return new ApiResponse<InstructorResponseDto> { Success = true, Message = "Instructor profile retrieved successfully.", Data = instructorDto };
        }
        public async Task<ApiResponse<InstructorResponseDto>> GetInstructorByUserIdAsync(int currentUserId)
        {
            if (currentUserId <= 0)
                throw new BadRequestException("Invalid Request.");

            var userData = await _unitOfWork.Users.FindAsync(u => u.UserId == currentUserId,
                includes: new[] { "Instructors" });

            if (userData == null || userData.IsDeleted == true)
                throw new NotFoundException("User account not found.");

            var instructor = userData.Instructors?.FirstOrDefault(i => !i.IsDeleted);

            if (instructor == null)
                throw new NotFoundException("This user is not registered as an instructor.");

            return await GetInstructorByIdAsync(instructor.InstructorId);
        }

        public async Task<ApiResponse<string>> UpdateInstructorAsync(int instructorId, UpdateInstructorDto request, int adminUserId)
        {
            if (instructorId <= 0 || request == null || adminUserId <= 0)
                throw new BadRequestException("Invalid Request.");

            var instructor = await _unitOfWork.Instructors.FindAsync(i => i.InstructorId == instructorId,
                includes: new[] { "User", "User.Person" });

            if (instructor == null || instructor.User.IsDeleted == true)
                throw new NotFoundException("Instructor not found.");

            instructor.Salary = request.Salary;

            if (instructor.User.Person != null)
            {
                instructor.User.Person.FirstName = request.FirstName;
                instructor.User.Person.SecondName = request.SecondName;
                instructor.User.Person.ThirdName = request.ThirdName;
                instructor.User.Person.LastName = request.LastName;
                instructor.User.Person.Phone = request.PhoneNumber;
            }

            _unitOfWork.Instructors.Update(instructor);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Admin ID: {AdminId} updated Instructor ID: {InstructorId}", adminUserId, instructorId);

            string cacheKey = $"instructor:{instructorId}";
            await _cache.RemoveAsync(cacheKey);

            return new ApiResponse<string> { Success = true, Message = "Instructor updated successfully.", Data = "Success" };
        }

        public async Task<ApiResponse<string>> DeleteInstructorAsync(int instructorId, int adminUserId)
        {
            if (instructorId <= 0 || adminUserId <= 0)
                throw new BadRequestException("Invalid Request.");

            var instructor = await _unitOfWork.Instructors.FindAsync(i => i.InstructorId == instructorId,
                includes: new[] { "User" } );

            if (instructor == null || (instructor.User != null && instructor.User.IsDeleted == true))
                throw new NotFoundException("Instructor not found or already deleted.");

            instructor.IsDeleted = true;
            instructor.UpdatedAt = DateTime.UtcNow;

            if (instructor.User != null)
            {
                instructor.User.IsDeleted = true;
                instructor.User.IsActive = false;
                instructor.User.UpdatedAt = DateTime.UtcNow;
                instructor.User.RefreshTokenHash = null;
                instructor.User.RefreshTokenExpiresAt = null;
                instructor.User.RefreshTokenRevokedAt = DateTime.UtcNow;
            }

            _unitOfWork.Instructors.Update(instructor);
            var affectedRows = await _unitOfWork.CompleteAsync();

            if (affectedRows <= 0)
                throw new BadRequestException("Failed to delete instructor.");

            _logger.LogWarning("Admin with User ID: {AdminId} has soft-deleted Instructor ID: {InstructorId}",
                adminUserId, instructorId);

            string cacheKey = $"instructor:{instructorId}";
            await _cache.RemoveAsync(cacheKey);

            return new ApiResponse<string> { Success = true, Message = "Instructor deleted and logged successfully.", Data = "Success" };
        }
    }
}