using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Linq.Expressions;
using System.Security.Claims;
using TrainingCenter.Business.Services;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.DTOs.People;
using TrainingCenter.Core.DTOs.Users;
using TrainingCenter.Core.Entities;
using TrainingCenter.Core.Interfaces.Services;

namespace TrainingCenter.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService UserService)
        {
            _userService = UserService;
        }

        [HttpGet("GetAll")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserResponseDto>>>> GetAllUsers([FromQuery] GetAllUsersRequestDto request)
        {
            var result = await _userService.GetAllAsync(request);

            return Ok(result);
        }

        [HttpGet("{userId:int}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetById(int userId,
            [FromServices] IAuthorizationService authorizationService)
        {
            var authResult = await authorizationService.AuthorizeAsync(User, userId, "OwnerOrAdmin");

            if (!authResult.Succeeded)
                return Forbid();

            var result = await _userService.GetUserByIdAsync(userId);

            return Ok(result);
        }

        [HttpPatch("Activate/{userId:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ActivateAccount(int userId)
        {
            var result = await _userService.ActivateAccountAsync(userId);

            return Ok(result);
        }

        [HttpPatch("Deactivate/{userId:int}")]
        [Authorize]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeactivateAccount(int userId, [FromServices] IAuthorizationService authorizationService)
        {
            var authResult = await authorizationService.AuthorizeAsync(User, userId, "OwnerOrAdmin");

            if (!authResult.Succeeded)
                return Forbid();

            var result = await _userService.DeactivateAccountAsync(userId);

            return Ok(result);
        }

        [HttpDelete("{userId:int}")]
        [Authorize]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteAccount(int userId, [FromServices] IAuthorizationService authorizationService)
        {
            var authResult = await authorizationService.AuthorizeAsync(User,userId, "OwnerOrAdmin");

            if (!authResult.Succeeded)
                return Forbid();

            var result = await _userService.DeleteAccountAsync(userId);

            return Ok(result);
        }

        [HttpPatch("Restore/{userId:int}")]
        [Authorize]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RestoreAccount(int userId, [FromServices] IAuthorizationService authorizationService)
        {
            var authResult = await authorizationService.AuthorizeAsync(User, userId, "OwnerOrAdmin");

            if (!authResult.Succeeded)
                return Forbid();

            var result = await _userService.RestoreAccountAsync(userId);

            return Ok(result);
        }

        [HttpPatch("ChangeRole/{userId:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ChangeUserRole(int userId, [FromBody] ChangeUserRoleDto request)
        {
            var result = await _userService.ChangeUserRoleAsync(userId, request);

            return Ok(result);
        }

        [HttpPut("change-password/{userId:int}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<string>>> ChangePassword(int userId,
            [FromBody] ChangePasswordDto request,[FromServices] IAuthorizationService authorizationService)
        {
            var authResult = await authorizationService.AuthorizeAsync(User, userId, "OwnerOrAdmin");

            if (!authResult.Succeeded)
                return Forbid();

            var result = await _userService.ChangePasswordAsync(userId, request);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType<ApiResponse<UserResponseDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<UserResponseDto>>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetMyProfile()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new ApiResponse<UserResponseDto> { Success = false, Message = "Invalid token claims." });

            return Ok(await _userService.GetUserByIdAsync(userId));
        }

        [Authorize]
        [HttpPatch("change-email")]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<string>>> ChangeEmail([FromBody] ChangeEmailDto request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new ApiResponse<UserResponseDto> { Success = false, Message = "Invalid token claims." });

            var result = await _userService.ChangeEmailAsync(request, userId);

            return Ok(result);
        }
    }
}