using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.DTOs.Enrollments;
using TrainingCenter.Core.Interfaces.Services;

namespace TrainingCenter.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;

        public EnrollmentsController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> EnrollInCourse([FromBody] EnrollmentRequestDto request)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _enrollmentService.EnrollInCourseAsync(request, currentUserId);
            return Ok(result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateEnrollment(int id, [FromBody] UpdateEnrollmentDto request)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _enrollmentService.UpdateEnrollmentAsync(id, request, adminId);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Student")]
        [ProducesResponseType<ApiResponse<EnrollmentResponseDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status403Forbidden)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetEnrollmentById(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var isAdmin = User.IsInRole("Admin");
            var result = await _enrollmentService.GetEnrollmentByIdAsync(id, currentUserId, isAdmin);
            return Ok(result);
        }

        [HttpGet("my-enrollments")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType<ApiResponse<IEnumerable<EnrollmentResponseDto>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetMyEnrollments()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _enrollmentService.GetMyEnrollmentsAsync(currentUserId);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<IEnumerable<EnrollmentResponseDto>>>(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllEnrollments([FromQuery] PagedFilterRequestDto request)
        {
            var result = await _enrollmentService.GetAllEnrollmentsAsync(request);
            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Student")]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Unenroll(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var isAdmin = User.IsInRole("Admin");

            var result = await _enrollmentService.UnenrollAsync(id, currentUserId, isAdmin);
            return Ok(result);
        }

        [HttpGet("course/{courseId:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<IEnumerable<EnrollmentResponseDto>>>(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetEnrollmentsByCourse(int courseId)
        {
            var result = await _enrollmentService.GetEnrollmentsByCourseIdAsync(courseId);
            return Ok(result);
        }
    }
}