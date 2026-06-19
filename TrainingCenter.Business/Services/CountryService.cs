using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.DTOs.Countries;
using TrainingCenter.Core.DTOs.Users;
using TrainingCenter.Core.Entities;
using TrainingCenter.Core.Exceptions;
using TrainingCenter.Core.Interfaces.Repositories;
using TrainingCenter.Core.Interfaces.Services;
using TrainingCenter.DataAccess.Repositories;

namespace TrainingCenter.Business.Services
{
    public class CountryService : ICountryService
    {
        private readonly IDistributedCache _cache;

        private readonly ILogger<CountryService> _logger;

        private readonly IUnitOfWork _unitOfWork;

        public CountryService(IDistributedCache cache,ILogger<CountryService> logger,IUnitOfWork unitOfWork)
        {
            _cache = cache;

            _logger = logger;

            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<IEnumerable<CountryResponseDto>>> GetAllCountriesAsync()
        {
            string cacheKey = "countries:all";

            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                var cachedDto = JsonSerializer.Deserialize<IEnumerable<CountryResponseDto>>(cachedData);
                return new ApiResponse<IEnumerable<CountryResponseDto>> { Success = true, Message = "Success from Cache.", Data = cachedDto };
            }

            var countries = await _unitOfWork.Countries.GetAllAsync();

            if (!countries.Any())
                return new ApiResponse<IEnumerable<CountryResponseDto>>
                {
                    Success = true,
                    Message = "No countries found.",
                    Data = Enumerable.Empty<CountryResponseDto>()
                };

            var result = countries.Select(c => new CountryResponseDto
            {
                CountryId = c.CountryId,
                CountryName = c.CountryName
            }).ToList();

            var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1) };
            var serializedData = JsonSerializer.Serialize(result);
            await _cache.SetStringAsync(cacheKey, serializedData, cacheOptions);

            _logger.LogInformation("Cache miss for countries:all. Fetched fresh data from database and populated cache.");

            return new ApiResponse<IEnumerable<CountryResponseDto>> { Success = true, Message = "Countries retrieved successfully.", Data = result };
        }

        public async Task<ApiResponse<CountryResponseDto>> GetCountryByIdAsync(int countryId)
        {
            if (countryId <= 0)
                throw new BadRequestException("Invalid ID.");

            string cacheKey = $"country:{countryId}";

            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                var cachedDto = JsonSerializer.Deserialize<CountryResponseDto>(cachedData);
                return new ApiResponse<CountryResponseDto> { Success = true, Message = "Success from Cache.", Data = cachedDto };
            }

            var country = await _unitOfWork.Countries.GetByIdAsync(countryId);

            if (country == null)
                throw new NotFoundException("Country not found.");

            var countryDto = new CountryResponseDto
            {
                CountryId = country.CountryId,
                CountryName = country.CountryName
            };

            var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1) };
            var serializedData = JsonSerializer.Serialize(countryDto);
            await _cache.SetStringAsync(cacheKey, serializedData, cacheOptions);

            _logger.LogInformation("Cache miss for country:{CountryId}. Fetched fresh data from database and populated cache.", countryId);

            return new ApiResponse<CountryResponseDto> { Success = true, Message = "Country retrieved successfully.", Data = countryDto };
        }

        public async Task<ApiResponse<CountryResponseDto>> SearchByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new BadRequestException("Invalid Name.");

            string cacheKey = $"country:name:{name.ToLower()}";

            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                var cachedDto = JsonSerializer.Deserialize<CountryResponseDto>(cachedData);
                return new ApiResponse<CountryResponseDto> { Success = true, Message = "Success from Cache.", Data = cachedDto };
            }

            var country = await _unitOfWork.Countries.FindAsync(c => c.CountryName.ToLower().Contains(name.ToLower()));

            if (country == null)
                throw new NotFoundException("Country not found.");

            var countryDto = new CountryResponseDto
            {
                CountryId = country.CountryId,
                CountryName = country.CountryName
            };

            var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1) };
            var serializedData = JsonSerializer.Serialize(countryDto);
            await _cache.SetStringAsync(cacheKey, serializedData, cacheOptions);

            _logger.LogInformation("Cache miss for country:name:{Name}. Fetched fresh data from database and populated cache.", name);

            return new ApiResponse<CountryResponseDto> { Success = true, Message = "Country retrieved successfully.", Data = countryDto };
        }
    }
}

