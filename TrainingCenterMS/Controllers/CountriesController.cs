using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.DTOs.Countries;
using TrainingCenter.Core.Interfaces.Services;

namespace TrainingCenter.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly ICountryService _countryService;

        public CountriesController(ICountryService countryService)
        {
            _countryService = countryService;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType<ApiResponse<IEnumerable<CountryResponseDto>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<IEnumerable<CountryResponseDto>>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<IEnumerable<CountryResponseDto>>>> GetAllCountries()
        {
            var result = await _countryService.GetAllCountriesAsync();

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType<ApiResponse<CountryResponseDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<CountryResponseDto>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<CountryResponseDto>>> GetById(int id)
        {
            var result = await _countryService.GetCountryByIdAsync(id);

            return Ok(result);
        }

        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType<ApiResponse<CountryResponseDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<CountryResponseDto>>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<CountryResponseDto>>> SearchByName([FromQuery] string name)
        {
            var result = await _countryService.SearchByNameAsync(name);

            return Ok(result);
        }
    }
}
