using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TrainingCenter.Core.Consts;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.DTOs.People;
using TrainingCenter.Core.DTOs.Users;
using TrainingCenter.Core.Entities;
using TrainingCenter.Core.Exceptions;
using TrainingCenter.Core.Interfaces.Repositories;
using TrainingCenter.Core.Interfaces.Services;
using TrainingCenter.DataAccess.Repositories;

namespace TrainingCenter.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _cache;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            IConfiguration configuration,
            IDistributedCache cache,
            ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _configuration = configuration;
            _cache = cache;
            _logger = logger;
        }
        public async Task<ApiResponse<IEnumerable<UserResponseDto>>> GetAllAsync(GetAllUsersRequestDto request)
        {
            Expression<Func<User, object>>? orderByExp = null;

            if (request.OrderBy != null)
            {
                orderByExp = request.OrderBy.ToLower() switch
                {
                    "userid" => u => u.UserId,
                    "email" => u => u.Email,
                    "isactive" => u => u.IsActive,
                    "createdat" => u => u.CreatedAt,
                    "lastlogindate" => u => u.LastLoginDate,
                    "roleid" => u => u.RoleId,
                    _ => null
                };
            }

            string[]? includes = request.IncludeInfo == true ? new[] { "Person" } : null;
            var direction = request.OrderByDirection?.ToLower().StartsWith("desc") == true ? "desc" : "asc";
            int? skip = request.Page.HasValue ? (request.Page - 1) * request.Limit : null;

            // استخدام _unitOfWork.Users بدل _userRepository
            var users = await _unitOfWork.Users.FindAllAsync(
                match: u => u.IsDeleted == false,
                includes: includes,
                Take: request.Limit,
                Skip: skip,
                orderBy: orderByExp,
                orderByDirection: direction
            );

            var result = users.Select(u => new UserResponseDto
            {
                UserId = u.UserId,
                Email = u.Email,
                IsActive = u.IsActive,
                IsEmailVerified = u.IsEmailVerified,
                CreatedAt = u.CreatedAt,
                LastLoginDate = u.LastLoginDate,
                RoleId = u.RoleId,
                Person = u.Person == null ? null : new PersonResponseDto
                {
                    PersonId = u.Person.PersonId,
                    NationalNo = u.Person.NationalNo,
                    FirstName = u.Person.FirstName,
                    SecondName = u.Person.SecondName,
                    ThirdName = u.Person.ThirdName,
                    LastName = u.Person.LastName,
                    DateOfBirth = u.Person.DateOfBirth,
                    Gender = u.Person.Gender,
                    Phone = u.Person.Phone,
                    NationalityCountryId = u.Person.NationalityCountryId
                }
            }).ToList();

            _logger.LogInformation("Successfully retrieved {Count} users. (Page: {Page}, Limit: {Limit}, OrderBy: {OrderBy} {Direction})",
                result.Count, request.Page ?? 1, request.Limit, request.OrderBy ?? "None", direction);

            return new ApiResponse<IEnumerable<UserResponseDto>>
            {
                Success = true,
                Message = "Data retrieved successfully.",
                Data = result
            };
        }
        public async Task<ApiResponse<UserResponseDto>> GetUserByIdAsync(int userId)
        {
            if (userId <= 0)
                throw new BadRequestException("Invalid ID.");

            string cacheKey = $"user:{userId}";

            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                var userDto = JsonSerializer.Deserialize<UserResponseDto>(cachedData);
                return new ApiResponse<UserResponseDto> { Success = true, Message = "Success from Cache.", Data = userDto };
            }

            string[] includes = new[] { "Person" };
            var user = await _unitOfWork.Users.FindAsync(u => u.UserId == userId, includes);

            if (user == null || user.IsDeleted == true)
                throw new NotFoundException("User not found.");

            var freshUserDto = new UserResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                IsActive = user.IsActive,
                IsEmailVerified = user.IsEmailVerified,
                CreatedAt = user.CreatedAt,
                LastLoginDate = user.LastLoginDate,
                RoleId = user.RoleId,
                Person = user.Person == null ? null : new PersonResponseDto
                {
                    PersonId = user.Person.PersonId,
                    NationalNo = user.Person.NationalNo,
                    FirstName = user.Person.FirstName,
                    SecondName = user.Person.SecondName,
                    ThirdName = user.Person.ThirdName,
                    LastName = user.Person.LastName,
                    DateOfBirth = user.Person.DateOfBirth,
                    Gender = user.Person.Gender,
                    Phone = user.Person.Phone,
                    NationalityCountryId = user.Person.NationalityCountryId
                }
            };

            var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) };
            var serializedData = JsonSerializer.Serialize(freshUserDto);
            await _cache.SetStringAsync(cacheKey, serializedData, cacheOptions);

            _logger.LogInformation("Cache miss for user:{UserId}. Fetched fresh data from database and populated cache.", userId);

            return new ApiResponse<UserResponseDto> { Success = true, Message = "Success.", Data = freshUserDto };
        }

        public async Task<ApiResponse<string>> ActivateAccountAsync(int userId)
        {
            if (userId <= 0)
                throw new BadRequestException("Invalid user ID.");

            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null || user.IsDeleted == true)
                throw new NotFoundException("User not found.");

            if (user.IsEmailVerified == false)
                throw new BadRequestException("Account cannot be activated because the email is not verified.");

            if (user.IsActive == true)
                throw new BadRequestException("Account is already active.");

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;

            user.RefreshTokenHash = null;
            user.RefreshTokenExpiresAt = null;
            user.RefreshTokenRevokedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            string cacheKey = $"user:{userId}";
            await _cache.RemoveAsync(cacheKey);

            _logger.LogInformation("User account with ID {UserId} has been successfully activated.", userId);

            return new ApiResponse<string> { Success = true, Message = "Account activated successfully." };
        }
        public async Task<ApiResponse<string>> DeactivateAccountAsync(int userId)
        {
            if (userId <= 0)
                throw new BadRequestException("Invalid user ID.");

            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null || user.IsDeleted == true)
                throw new NotFoundException("User not found.");

            if (user.IsActive == false)
                throw new BadRequestException("Account is already deactivated.");

            if (user.IsEmailVerified == false)
                throw new BadRequestException("Account cannot be deactivated because the email is not verified.");

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            user.RefreshTokenHash = null;
            user.RefreshTokenExpiresAt = null;
            user.RefreshTokenRevokedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            string cacheKey = $"user:{userId}";
            await _cache.RemoveAsync(cacheKey);

            _logger.LogInformation("User account with ID {UserId} has been successfully deactivated and sessions were revoked.", userId);

            return new ApiResponse<string> { Success = true, Message = "Account deactivated successfully." };
        }
        public async Task<ApiResponse<string>> DeleteAccountAsync(int userId)
        {
            if (userId <= 0)
                throw new BadRequestException("Invalid user ID.");

            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null || user.IsDeleted == true)
                throw new NotFoundException("User not found.");

            user.IsDeleted = true;
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            user.RefreshTokenHash = null;
            user.RefreshTokenExpiresAt = null;
            user.RefreshTokenRevokedAt = DateTime.UtcNow;

            user.VerificationToken = null;
            user.VerificationTokenExpiresAt = null;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            string cacheKey = $"user:{userId}";
            await _cache.RemoveAsync(cacheKey);

            _logger.LogInformation("User account with ID {UserId} has been successfully soft-deleted and all active sessions were revoked.", userId);

            return new ApiResponse<string> { Success = true, Message = "User deleted successfully." };
        }
        public async Task<ApiResponse<string>> RestoreAccountAsync(int userId)
        {
            if (userId <= 0)
                throw new BadRequestException("Invalid user ID.");

            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null || user.IsDeleted == false)
                throw new NotFoundException("User not found or account is not deleted.");

            user.IsDeleted = false;
            user.IsActive = user.IsEmailVerified == true;
            user.UpdatedAt = DateTime.UtcNow;

            user.RefreshTokenHash = null;
            user.RefreshTokenExpiresAt = null;
            user.RefreshTokenRevokedAt = DateTime.UtcNow;

            user.VerificationToken = null;
            user.VerificationTokenExpiresAt = null;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            string cacheKey = $"user:{userId}";
            await _cache.RemoveAsync(cacheKey);

            _logger.LogInformation("User account with ID {UserId} has been successfully restored and reactivated.", userId);

            return new ApiResponse<string> { Success = true, Message = "Account restored successfully." };
        }
        public async Task<ApiResponse<string>> ChangeUserRoleAsync(int userId, ChangeUserRoleDto request)
        {
            if (userId <= 0)
                throw new BadRequestException("Invalid user ID.");

            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null || user.IsDeleted == true)
                throw new NotFoundException("User not found.");

            if (user.IsActive == false)
                throw new BadRequestException("Account is deactivated. Cannot change role.");

            if (user.IsEmailVerified == false)
                throw new BadRequestException("Your email is not verified yet.");

            if (user.RoleId == request.NewRoleId)
                throw new BadRequestException("User is already assigned to this role.");

            var roleExists = await _unitOfWork.Roles.ExistsAsync(r => r.RoleId == request.NewRoleId);

            if (!roleExists)
                throw new NotFoundException("The specified Role does not exist.");

            var oldRoleId = user.RoleId;

            user.RoleId = request.NewRoleId;
            user.UpdatedAt = DateTime.UtcNow;

            user.RefreshTokenHash = null;
            user.RefreshTokenExpiresAt = null;
            user.RefreshTokenRevokedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            string cacheKey = $"user:{userId}";
            await _cache.RemoveAsync(cacheKey);

            _logger.LogInformation("Successfully changed role for user {UserId} from RoleId {OldRoleId} to {NewRoleId}. All active sessions have been revoked.",
                userId, oldRoleId, request.NewRoleId);

            return new ApiResponse<string> { Success = true, Message = "User role updated successfully." };
        }
        public async Task<ApiResponse<string>> ChangePasswordAsync(int userId, ChangePasswordDto request)
        {
            if (userId <= 0)
                throw new BadRequestException("Invalid user ID.");

            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null || user.IsDeleted == true)
                throw new NotFoundException("User not found.");

            if (user.IsActive == false)
                throw new BadRequestException("Account is deactivated.");

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);

            if (!isPasswordValid)
                throw new BadRequestException("Current password is incorrect.");

            if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash))
                throw new BadRequestException("New password must be different from the current password.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            user.RefreshTokenHash = null;
            user.RefreshTokenExpiresAt = null;
            user.RefreshTokenRevokedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            string cacheKey = $"user:{userId}";
            await _cache.RemoveAsync(cacheKey);

            _logger.LogInformation("User {UserId} successfully changed their password. All active sessions/refresh tokens have been revoked.", userId);

            return new ApiResponse<string> { Success = true, Message = "Password changed successfully." };
        }
        public async Task<ApiResponse<string>> ChangeEmailAsync(ChangeEmailDto request, int userId)
        {
            if (userId <= 0)
                throw new BadRequestException("Invalid user ID.");

            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null || user.IsDeleted == true)
                throw new NotFoundException("User not found.");

            if (user.IsActive == false)
                throw new BadRequestException("Account is deactivated.");

            if (user.IsEmailVerified == false)
                throw new BadRequestException("Current email address must be verified before changing it.");

            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
            if (!isPasswordCorrect)
                throw new BadRequestException("Current password is incorrect.");

            var emailExists = await _unitOfWork.Users.ExistsAsync(e => e.Email == request.NewEmail && e.UserId != userId);
            if (emailExists)
                throw new BadRequestException("This email is already taken.");

            user.Email = request.NewEmail;
            user.IsEmailVerified = false;

            var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            user.VerificationToken = BCrypt.Net.BCrypt.HashPassword(otp);

            var expiryMinutes = int.Parse(_configuration[OtpSettingsKeys.ExpiryMinutes] ?? "15");
            user.VerificationTokenExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            user.RefreshTokenHash = null;
            user.RefreshTokenExpiresAt = null;
            user.RefreshTokenRevokedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            await _notificationService.SendAsync(request.NewEmail, "Verify Your New Email", $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #2c3e50;'>Verify Your New Email Address</h2>
                        <p>You have requested to change your current email. Your account verification code is:</p>
                        <h1 style='color: #3498db; letter-spacing: 8px; text-align: center;'>{otp}</h1>
                        <p style='color: #888;'>This code will expire in <strong>{expiryMinutes} minutes</strong>.</p>
                        <hr style='border: none; border-top: 1px solid #eee;'>
                        <p style='color: #e74c3c; font-size: 13px;'>
                            ⚠️ If you did not request this email change, please ignore this message and secure your account.
                        </p>
                    </div>");

            await _cache.RemoveAsync($"user:{userId}");

            _logger.LogInformation("User {UserId} successfully requested an email change to {NewEmail}.", userId, request.NewEmail);

            return new ApiResponse<string> { Success = true, Message = "Email updated successfully. Please verify your new email." };
        }
    }
}
