using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.DTOs.Countries;

namespace TrainingCenter.Core.Interfaces.Services
{
    public interface ICountryService
    {
        Task<ApiResponse<IEnumerable<CountryResponseDto>>> GetAllCountriesAsync();

        Task<ApiResponse<CountryResponseDto>> GetCountryByIdAsync(int countryId);

        Task<ApiResponse<CountryResponseDto>> SearchByNameAsync(string name);
    }
}
