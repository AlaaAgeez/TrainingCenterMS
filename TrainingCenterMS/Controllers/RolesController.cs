using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.DTOs.Roles;
using TrainingCenter.Core.Interfaces.Services;

namespace TrainingCenter.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<IEnumerable<RoleResponseDto>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<IEnumerable<RoleResponseDto>>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<IEnumerable<RoleResponseDto>>>> GetAllRoles()
        {
            var result = await _roleService.GetAllRolesAsync();

            return Ok(result);
        }

        [HttpGet("{id:min(1)}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<RoleResponseDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<RoleResponseDto>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<RoleResponseDto>>> GetRoleById(byte id)
        {
            var result = await _roleService.GetRoleByIdAsync(id);

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status201Created)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<string>>> CreateRole([FromBody] CreateRoleRequestDto request)
        {
            var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown Admin";

            var result = await _roleService.CreateRoleAsync(request, adminEmail);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<string>>> UpdateRole([FromBody] UpdateRoleRequestDto request)
        {
            var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown Admin";

            var result = await _roleService.UpdateRoleNameAsync(request, adminEmail);

            return Ok(result);
        }

        [HttpDelete("{id:min(1)}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<string>>> DeleteRole(byte id)
        {
            var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown Admin";

            var result = await _roleService.DeleteRoleAsync(id, adminEmail);

            return Ok(result);
        }
    }
}
