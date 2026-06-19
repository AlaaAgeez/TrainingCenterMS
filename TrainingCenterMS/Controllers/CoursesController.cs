using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.DTOs.Courses;
using TrainingCenter.Core.Interfaces.Services;

namespace TrainingCenter.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ICoursesService _coursesService;

        public CoursesController(ICoursesService coursesService)
        {
            _coursesService = coursesService;
        }

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status201Created)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> CreateCourse([FromBody] CreateCourseRequestDto request)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var result = await _coursesService.CreateCourseAsync(request, currentUserId);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpGet("all")] 
        [AllowAnonymous]
        [ProducesResponseType<ApiResponse<IEnumerable<CourseResponseDto>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetAllCourses([FromQuery] PagedFilterRequestDto request)
        {
            var result = await _coursesService.GetAllCoursesAsync(request);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType<ApiResponse<CourseResponseDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)] 
        public async Task<ActionResult> GetCourseById(int id)
        {
            var result = await _coursesService.GetCourseByIdAsync(id);

            return Ok(result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Instructor")]
        [ProducesResponseType<ApiResponse<CourseResponseDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateCourse(int id, [FromBody] UpdateCourseRequestDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown";

            var result = await _coursesService.UpdateCourseAsync(id, request, userId, userRole);

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteCourse(int id)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            var adminRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown";

            var result = await _coursesService.DeleteCourseAsync(id, adminId, adminRole);

            return Ok(result);
        }

        [HttpGet("my-courses")]
        [Authorize(Roles = "Instructor")]
        [ProducesResponseType<ApiResponse<IEnumerable<CourseResponseDto>>>(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetMyCourses([FromQuery] PagedFilterRequestDto request)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var result = await _coursesService.GetCoursesByInstructorAsync(request, currentUserId);

            return Ok(result);
        }

        [HttpPatch("{id:int}/status")]
        [Authorize(Roles = "Admin,Instructor")]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        public async Task<ActionResult> ChangeStatus(int id, [FromBody] string newStatus)
        {
            var result = await _coursesService.ChangeCourseStatusAsync(id, newStatus);

            return Ok(result);
        }

        [HttpGet("instructor/{instructorId:int}")]
        [AllowAnonymous]
        [ProducesResponseType<ApiResponse<IEnumerable<CourseResponseDto>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetCoursesByInstructorId(int instructorId, [FromQuery] PagedFilterRequestDto request)
        {
            var result = await _coursesService.GetCoursesByInstructorIdAsync(request, instructorId);

            return Ok(result);
        }

        [HttpGet("deleted")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<IEnumerable<CourseResponseDto>>>(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetDeletedCourses([FromQuery] PagedFilterRequestDto request)
        {
            var result = await _coursesService.GetDeletedCoursesAsync(request);

            return Ok(result);
        }

        [HttpPatch("{id:int}/restore")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RestoreCourse(int id)
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";

            var result = await _coursesService.RestoreCourseAsync(id, adminId);

            return Ok(result);
        }
    }
}
