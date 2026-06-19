using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.DTOs.Students;
using TrainingCenter.Core.Interfaces.Services;

namespace TrainingCenter.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<IEnumerable<StudentResponseDto>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetAllStudents([FromQuery] PagedFilterRequestDto request)
        {
            var result = await _studentService.GetAllStudentsAsync(request);
            return Ok(result);
        }

        [HttpGet("profile")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType<ApiResponse<StudentResponseDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetMyProfile()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _studentService.GetStudentProfileAsync(currentUserId);
            return Ok(result);
        }

        [HttpPatch("{id:int}/change-status")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ChangeStatus(int id, [FromQuery] string newStatus)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _studentService.ChangeStatusAsync(id, newStatus, adminId);
            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteStudent(int id)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _studentService.DeleteStudentAsync(id, adminId);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<StudentResponseDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetStudentById(int id)
        {
            var result = await _studentService.GetStudentByIdAsync(id);
            return Ok(result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Student")]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateStudent(int studentId, [FromBody] UpdateStudentDto updateDto)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _studentService.UpdateStudentAsync(studentId, updateDto, currentUserId);
            return Ok(result);
        }
    }
}