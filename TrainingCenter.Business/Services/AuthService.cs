using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TrainingCenter.Core.Consts;
using TrainingCenter.Core.DTOs.Auth;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.Entities;
using TrainingCenter.Core.Exceptions;
using TrainingCenter.Core.Interfaces.Repositories;
using TrainingCenter.Core.Interfaces.Services;
using TrainingCenter.DataAccess.Repositories;

namespace TrainingCenter.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;

        private readonly ILogger<AuthService> _logger;

        private readonly INotificationService _notificationService;

        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IConfiguration configuration,ILogger<AuthService> logger,INotificationService notificationService,
            IUnitOfWork unitOfWork)
        {
            _configuration = configuration;

            _logger = logger;

            _notificationService = notificationService;

            _unitOfWork = unitOfWork;
        }

        private string HashRefreshToken(string token)
        {
            var tokenBytes = Encoding.UTF8.GetBytes(token);
            var hashBytes = SHA256.HashData(tokenBytes);
            return Convert.ToBase64String(hashBytes);
        }

        private string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            return Convert.ToBase64String(bytes);
        }

        private string GenerateAccessToken(User user)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.PersonId.ToString()),
            new Claim("PersonId", user.PersonId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "No Role")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration[JwtSettingsKeys.SecretKey]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            int expiryMinutes = int.Parse(_configuration[JwtSettingsKeys.AccessTokenExpiryMinutes] ?? "30");

            var token = new JwtSecurityToken(
                    issuer: _configuration[JwtSettingsKeys.Issuer],
                    audience: _configuration[JwtSettingsKeys.Audience],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                    signingCredentials: creds );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<ApiResponse<TokenResponseDto>> LoginAsync(LoginRequestDto request)
        {
            if (request == null)
                throw new UnauthorizedException("Invalid Request.");

            var user = await _unitOfWork.Users.FindAsync(x => x.Email == request.Email, includes: new[] { "Role" });

            bool isValid = user != null && BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (user == null || !isValid)
            {
                _logger.LogWarning("Failed login attempt for {Email}", request.Email);
                throw new NotFoundException("Email or password is incorrect.");
            }

            if (user.IsDeleted == true) 
                throw new UnauthorizedException("Account has been deleted.");

            if (user.IsActive == false) 
                throw new UnauthorizedException("Account is not active.");

            if (user.IsEmailVerified == false)
                throw new UnauthorizedException("Email is not verified.");

            var tokenString = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshTokenHash = HashRefreshToken(refreshToken);

            int expiryDays = int.Parse(_configuration[JwtSettingsKeys.RefreshTokenExpiryDays] ?? "7");
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(expiryDays);
            user.RefreshTokenRevokedAt = null;
            user.LastLoginDate = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("User {Email} with Role {RoleName} logged in successfully.",
                user.Email,
                user.Role?.RoleName ?? "No Role");

            return new ApiResponse<TokenResponseDto>
            {
                Success = true,
                Message = "Login successful.",
                Data = new TokenResponseDto
                {
                    AccessToken = tokenString,
                    RefreshToken = refreshToken
                }
            };
        }

        public async Task<ApiResponse<string>> RegisterStaffAsync(RegisterRequestDto request, string adminEmail)
        {
            if (request.RoleId == 2)
                throw new BadRequestException("Cannot register a student from this endpoint.");

            var existingUser = await _unitOfWork.Users.FindAsync(x => x.Email == request.Email);

            if (existingUser != null)
                throw new ConflictException("Email already exists.");

            var newUser = new User
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RoleId = request.RoleId,
                IsActive = true,
                IsEmailVerified = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(newUser);

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Admin {AdminEmail} registered a new staff member with Email: {NewEmail} and RoleId: {RoleId}.",
                      adminEmail,
                      request.Email,
                      request.RoleId);

            return new ApiResponse<string> { Success = true, Message = "User registered successfully." };
        }

        public async Task<ApiResponse<string>> RegisterStudentAsync(RegisterStudentDto request)
        {
            var existingUser = await _unitOfWork.Users.FindAsync(x => x.Email == request.Email);

            if (existingUser != null)
            {
                if (existingUser.IsEmailVerified == true)
                    throw new ConflictException("Email already exists.");

                if (existingUser.VerificationTokenExpiresAt > DateTime.UtcNow && existingUser.IsEmailVerified == false)
                    throw new ConflictException("An active verification code has already been sent to this email.");
                
                await _unitOfWork.Users.DeleteAsync(existingUser);
            }

            var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            var expiryMinutes = int.Parse(_configuration["OtpSettings:ExpiryMinutes"] ?? "15");

            var user = new User
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RoleId = 2,
                IsActive = true,
                IsEmailVerified = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                VerificationToken = BCrypt.Net.BCrypt.HashPassword(otp),
                VerificationTokenExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes)
            };

            try
            {
                await _notificationService.SendAsync(user.Email, "Verify Your Email", $@"
               <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                   <h2 style='color: #2c3e50;'>Verify Your Email Address</h2>
                   <p>Your verification code is:</p>
                   <h1 style='color: #3498db; letter-spacing: 8px; text-align: center;'>{otp}</h1>
                   <p style='color: #888;'>This code will expire in <strong>{expiryMinutes} minutes</strong>.</p>
                   <hr style='border: none; border-top: 1px solid #eee;'>
                   <p style='color: #e74c3c; font-size: 13px;'>
                       ⚠️ If you did not request this, please ignore this email.
                   </p>
               </div>");

                await _unitOfWork.Users.AddAsync(user);

                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email. Email: {Email}", user.Email);
                throw;
            }

            return new ApiResponse<string> { Success = true, Message = "User registered successfully." };
        }

        public async Task<ApiResponse<TokenResponseDto>> RefreshTokenAsync(RefreshRequestDto request, string email)
        {
            var user = await _unitOfWork.Users.FindAsync(x => x.Email == email, includes: new[] { "Role" });

            if (user == null)
                throw new NotFoundException("User not found.");

            if (user.RefreshTokenRevokedAt != null)
                throw new UnauthorizedException("Refresh token has been revoked.");

            if (user.RefreshTokenHash == null)
                throw new UnauthorizedException("Refresh token is invalid.");

            if (user.RefreshTokenExpiresAt == null || user.RefreshTokenExpiresAt <= DateTime.UtcNow)
                throw new UnauthorizedException("Refresh token has expired. Please login again.");

            string hashedInputRefreshToken = HashRefreshToken(request.RefreshToken);

            if (user.RefreshTokenHash != hashedInputRefreshToken)
                throw new UnauthorizedException("Invalid refresh token.");

            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshTokenHash = HashRefreshToken(newRefreshToken);

            int expiryDays = int.Parse(_configuration[JwtSettingsKeys.RefreshTokenExpiryDays] ?? "7");
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(expiryDays);

            user.RefreshTokenRevokedAt = null;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Token refreshed successfully. Email: {Email}", email);

            return new ApiResponse<TokenResponseDto>
            {
                Success = true,
                Message = "Token refreshed successfully.",
                Data = new TokenResponseDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                }
            };
        }

        public async Task<ApiResponse<string>> LogoutAsync(LogoutRequestDto request, string email)
        {
            var user = await _unitOfWork.Users.FindAsync(x => x.Email == email);

            if (user == null)
                throw new NotFoundException("User not found.");

            string hashedInputRefreshToken = HashRefreshToken(request.RefreshToken);

            if (hashedInputRefreshToken != user.RefreshTokenHash)
                throw new UnauthorizedException("Invalid refresh token.");

            user.RefreshTokenHash = null;
            user.RefreshTokenExpiresAt = null;
            user.RefreshTokenRevokedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("User logged out successfully. Email: {Email}", email);

            return new ApiResponse<string> { Success = true, Message = "Logged out successfully." };
        }

        public async Task<ApiResponse<string>> SendVerificationEmailAsync(ResendVerificationEmailDto request)
        {
            var user = await _unitOfWork.Users.FindAsync(x => x.Email == request.Email);

            if (user == null || user.IsDeleted == true)
            {
                _logger.LogWarning("Resend OTP failed: Email not found or deleted. Email: {Email}", request.Email);
                throw new NotFoundException("Email not found.");
            }

            if (user.IsEmailVerified == true)
                throw new BadRequestException("Email already verified.");

            if (user.IsActive == false)
            {
                _logger.LogWarning("Resend OTP failed: Inactive account attempted to request OTP. Email: {Email}", request.Email);
                throw new BadRequestException("Account is not active.");
            }

            if (user.VerificationTokenExpiresAt != null && user.VerificationTokenExpiresAt > DateTime.UtcNow)
                throw new BadRequestException("A verification code was already sent. Please check your email.");


            var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            user.VerificationToken = BCrypt.Net.BCrypt.HashPassword(otp);

            var expiryMinutes = int.Parse(_configuration[OtpSettingsKeys.ExpiryMinutes] ?? "15");
            user.VerificationTokenExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            _unitOfWork.Users.Update(user);

            try
            {
                await _notificationService.SendAsync(user.Email, "Verify Your Email", $@"
           <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
               <h2 style='color: #2c3e50;'>Verify Your Email Address</h2>
               <p>Your verification code is:</p>
               <h1 style='color: #3498db; letter-spacing: 8px; text-align: center;'>{otp}</h1>
               <p style='color: #888;'>This code will expire in <strong>{expiryMinutes} minutes</strong>.</p>
               <hr style='border: none; border-top: 1px solid #eee;'>
               <p style='color: #e74c3c; font-size: 13px;'>
                   ⚠️ If you did not request this, please ignore this email.
               </p>
           </div>");

                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email via NotificationService. Email: {Email}", request.Email);
                throw;
            }

            _logger.LogInformation("Verification email sent. Email: {Email}", request.Email);


            return new ApiResponse<string> { Success = true, Message = "Verification email sent." };
        }

        public async Task<ApiResponse<string>> VerifyEmailAsync(VerifyEmailRequestDto request)
        {
            var user = await _unitOfWork.Users.FindAsync(x => x.Email == request.Email);

            if (user == null || user.IsDeleted == true)
                throw new NotFoundException("Email not found.");

            if (user.IsEmailVerified == true)
                throw new BadRequestException("Email already verified.");

            if (user.VerificationTokenExpiresAt <= DateTime.UtcNow)
                throw new BadRequestException("OTP has expired.");

            if (!BCrypt.Net.BCrypt.Verify(request.Otp, user.VerificationToken))
                throw new BadRequestException("Invalid OTP.");

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                user.IsEmailVerified = true;
                user.VerificationToken = null;
                user.VerificationTokenExpiresAt = null;
                user.IsActive = true;

                _unitOfWork.Users.Update(user);

                if (user.RoleId == 2)
                {
                    await _unitOfWork.Students.AddAsync(new Student
                    {
                        UserId = user.UserId,
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow
                    });
                }
                else if (user.RoleId == 3)
                {
                    await _unitOfWork.Instructors.AddAsync(new Instructor
                    {
                        UserId = user.UserId,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _unitOfWork.CompleteAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
            return new ApiResponse<string> { Success = true, Message = "Email verified successfully." };
        }

        public async Task<ApiResponse<string>> ChangePasswordAsync(int userId, ChangePasswordRequestDto request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null || user.IsDeleted == true)
                throw new NotFoundException("User not found.");

            if (request.NewPassword == request.CurrentPassword)
                throw new BadRequestException("New password must be different from current password.");

            bool isValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);

            if (!isValid)
            {
                _logger.LogWarning("Invalid password attempt for user {UserId} - {Email}.", user.UserId, user.Email);
                throw new BadRequestException("Current password is incorrect.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("User {Email} changed their password", user.Email);

            return new ApiResponse<string> { Success = true, Message = "Password changed successfully." };
        }

        public async Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            var user = await _unitOfWork.Users.FindAsync(x => x.Email == request.Email);

            if (user == null || user.IsDeleted == true)
                throw new NotFoundException("Email not found.");

            if (user.IsActive == false)
                throw new BadRequestException("Account is not active.");

            if (user.IsEmailVerified == false)
                throw new BadRequestException("Email is not verified.");

            if (user.VerificationTokenExpiresAt != null && user.VerificationTokenExpiresAt > DateTime.UtcNow)
                throw new BadRequestException("A reset code was already sent. Please check your email.");

            var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            user.VerificationToken = BCrypt.Net.BCrypt.HashPassword(otp);

            var expiryMinutes = int.Parse(_configuration[OtpSettingsKeys.ExpiryMinutes] ?? "15");
            user.VerificationTokenExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            try
            {
                await _notificationService.SendAsync(user.Email, "Reset Your Password", $@"
                  <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                      <h2 style='color: #2c3e50;'>Reset Your Password</h2>
                      <p>We received a request to reset your password. Your reset code is:</p>
                      <h1 style='color: #3498db; letter-spacing: 8px; text-align: center;'>{otp}</h1>
                      <p style='color: #888;'>This code will expire in <strong>{expiryMinutes} minutes</strong>.</p>                      <hr style='border: none; border-top: 1px solid #eee;'>
                      <p style='color: #e74c3c; font-size: 13px;'>
                          ⚠️ If you did not request a password reset, please ignore this email.
                      </p>
                  </div>");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email via NotificationService. Email: {Email}", request.Email);
                throw;
            }

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Password reset email sent for user {UserId} - {Email}.", user.UserId, user.Email);

            return new ApiResponse<string> { Success = true, Message = "Password reset email sent." };
        }

        public async Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            var user = await _unitOfWork.Users.FindAsync(x => x.Email == request.Email);

            if (user == null || user.IsDeleted == true)
                throw new NotFoundException("Email not found.");

            if (user.IsActive == false)
                throw new UnauthorizedException("Account is not active.");

            if (user.IsEmailVerified == false)
                throw new UnauthorizedException("Email is not verified.");

            if (user.VerificationToken == null || user.VerificationTokenExpiresAt == null)
                throw new BadRequestException("No reset token found.");

            if (user.VerificationTokenExpiresAt <= DateTime.UtcNow)
                throw new UnauthorizedException("OTP has expired.");

            if (!BCrypt.Net.BCrypt.Verify(request.Otp, user.VerificationToken))
            {
                _logger.LogWarning("Invalid OTP attempt for user {UserId} - {Email}.", user.UserId, user.Email);
                throw new UnauthorizedException("Invalid OTP.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            user.VerificationToken = null;
            user.VerificationTokenExpiresAt = null;
            user.IsActive = true;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("User {UserId} - {Email} reset their password successfully.", user.UserId, user.Email);

            return new ApiResponse<string> { Success = true, Message = "Password changed successfully." };
        }

        public async Task<ApiResponse<string>> RevokeTokenAsync(int userId, string adminEmail)
        {
            if (userId <= 0) 
                throw new BadRequestException("Invalid user id.");

            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null || user.IsDeleted == true)
                throw new NotFoundException("User not found.");

            if (user.IsActive == false)
                throw new UnauthorizedException("Cannot revoke token. Account is already deactivated.");

            if (user.IsEmailVerified == false)
                throw new UnauthorizedException("Cannot revoke token. User email is not verified yet.");

            if (string.IsNullOrEmpty(user.RefreshTokenHash) || user.RefreshTokenExpiresAt <= DateTime.UtcNow)
            {
                _logger.LogWarning("Admin {AdminEmail} attempted to revoke token for user {UserId} but no active session found.", adminEmail, user.UserId);
                throw new BadRequestException("User does not have an active session or token is already expired.");
            }

            user.RefreshTokenHash = null;
            user.RefreshTokenExpiresAt = null;
            user.RefreshTokenRevokedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Admin {AdminEmail} revoked token for user {UserId} - {Email}.", adminEmail, user.UserId, user.Email);

            return new ApiResponse<string> { Success = true, Message = "User tokens revoked successfully." };
        }
    }
}
