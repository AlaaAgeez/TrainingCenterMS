using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrainingCenter.Business.Authorization;
using TrainingCenter.Business.Services;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.DTOs.Instructors;
using TrainingCenter.Core.Interfaces.Services;

namespace TrainingCenter.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstructorsController : ControllerBase
    {
        private readonly IInstructorsService _instructorsService;

        public InstructorsController(IInstructorsService instructorsService)
        {
            _instructorsService = instructorsService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status201Created)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> CreateInstructor([FromBody] CreateInstructorRequestDto request)
        {
            var adminUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var result = await _instructorsService.CreateInstructor(request, adminUserId);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<IEnumerable<InstructorResponseDto>>>(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllInstructors([FromQuery] PagedFilterRequestDto request)
        {
            var result = await _instructorsService.GetAllInstructorsAsync(request);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<InstructorResponseDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetInstructorById(int id)
        {
            var result = await _instructorsService.GetInstructorByIdAsync(id);

            return Ok(result);
        }

        [HttpGet("my-profile")]
        [Authorize(Roles = "Instructor")]
        [ProducesResponseType<ApiResponse<InstructorResponseDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetMyProfile()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var result = await _instructorsService.GetInstructorByUserIdAsync(currentUserId);

            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateInstructor(int id, [FromBody] UpdateInstructorDto request)
        {
            var adminUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var result = await _instructorsService.UpdateInstructorAsync(id, request, adminUserId);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> DeleteInstructor(int id)
        {
            var adminUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            var result = await _instructorsService.DeleteInstructorAsync(id, adminUserId);

            return Ok(result);
        }
    }
}
