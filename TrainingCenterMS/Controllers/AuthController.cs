using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrainingCenter.Core.DTOs.Auth;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.Interfaces.Services;

namespace TrainingCenterMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType<ApiResponse<TokenResponseDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<TokenResponseDto>>(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);

            return Ok(result);
        }

        [HttpPost("register/staff")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status201Created)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RegisterStaff([FromBody] RegisterRequestDto request)
        {
            var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown Admin";

            var result = await _authService.RegisterStaffAsync(request, adminEmail);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPost("register/student")]
        [AllowAnonymous]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status201Created)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<string>>> RegisterStudent([FromBody] RegisterStudentDto request)
        {
            var result = await _authService.RegisterStudentAsync(request);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPost("refresh")]
        [Authorize]
        [ProducesResponseType<ApiResponse<TokenResponseDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<TokenResponseDto>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Refresh([FromBody] RefreshRequestDto request)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var result = await _authService.RefreshTokenAsync(request, email);

            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Logout([FromBody] LogoutRequestDto request)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var result = await _authService.LogoutAsync(request, email);

            return Ok(result);
        }

        [HttpPost("resend-verification-email")]
        [AllowAnonymous]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ResendVerificationEmail([FromBody] ResendVerificationEmailDto request)
        {
            var result = await _authService.SendVerificationEmailAsync(request);

            return Ok(result);
        }

        [HttpPost("verify-email")]
        [AllowAnonymous]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> VerifyEmail([FromBody] VerifyEmailRequestDto request)
        {
            var result = await _authService.VerifyEmailAsync(request);

            return Ok(result);
        }

        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
                return Unauthorized();

            var result = await _authService.ChangePasswordAsync(userId, request);

            return Ok(result);
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            var result = await _authService.ForgotPasswordAsync(request);

            return Ok(result);
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            var result = await _authService.ResetPasswordAsync(request);

            return Ok(result);
        }

        [HttpPost("revoke-token/{userId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RevokeToken([FromRoute] int userId)
        {
            var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(adminEmail))
                return Unauthorized();

            var result = await _authService.RevokeTokenAsync(userId, adminEmail);

            return Ok(result);
        }
    }
}
