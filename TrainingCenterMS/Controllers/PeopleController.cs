using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrainingCenter.Business.Services;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.DTOs.People;
using TrainingCenter.Core.Entities;
using TrainingCenter.Core.Interfaces.Services;

namespace TrainingCenter.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeopleController : ControllerBase
    {
        private readonly IPeopleService _peopleService;

        public PeopleController(IPeopleService peopleService)
        {
            _peopleService = peopleService;
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<string>>> AddNewPerson([FromBody] PersonRequestDto request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _peopleService.AddNewPersonasync(request, userId);

            return Ok(result);
        }

        [HttpPut("me")]
        [Authorize]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProfile([FromBody] PersonRequestDto request)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _peopleService.UpdateMyProfileAsync(currentUserId, request);

            return Ok(result);
        }

        [HttpPut("{personId:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ApiResponse<string>>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<string>>> UpdatePersonByAdmin(int personId, [FromBody] PersonRequestDto request)
        {
            var CurruntAmdin = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _peopleService.UpdatePersonByAdminAsync(personId, request, CurruntAmdin);

            return Ok(result);
        }

        [HttpGet("{personId:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<PersonResponseDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<PersonResponseDto>>(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<PersonResponseDto>>> GetPersonByID(int personId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isAdmin = User.IsInRole("Admin");

            var result = await _peopleService.GetPersonByIDAsync(personId, currentUserId, isAdmin);

            return Ok(result);
        }

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType<ApiResponse<PersonResponseDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<PersonResponseDto>>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PersonResponseDto>>> GetMyProfile()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _peopleService.GetMyProfileAsync(currentUserId);

            return Ok(result);
        }

        [HttpGet("GetAll")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<ApiResponse<IEnumerable<PersonResponseDto>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<IEnumerable<PersonResponseDto>>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PersonResponseDto>>>> GetAllPersons([FromQuery] PagedFilterRequestDto request)
        {
            var result = await _peopleService.GetAllPeopleAsync(request);

            return Ok(result);
        }
    }
}