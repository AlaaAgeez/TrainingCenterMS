using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.DTOs.Countries;
using TrainingCenter.Core.DTOs.People;
using TrainingCenter.Core.DTOs.Users;
using TrainingCenter.Core.Entities;
using TrainingCenter.Core.Exceptions;
using TrainingCenter.Core.Interfaces.Repositories;
using TrainingCenter.Core.Interfaces.Services;

namespace TrainingCenter.Business.Services
{
    public class PeopleService : IPeopleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _cache;
        private readonly ILogger<Person> _logger;

        public PeopleService(IUnitOfWork unitOfWork, IDistributedCache cache, ILogger<Person> logger)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> AddNewPersonasync(PersonRequestDto request, int userId)
        {
            if (userId <= 0)
                throw new BadRequestException("Invalid Request.");

            var user = await _unitOfWork.Users.GetWithTrackingAsync(u => u.UserId == userId);

            if (user == null)
                throw new NotFoundException("User Not Found");

            if (user.PersonId != null)
                throw new ConflictException("User Has Person Data Already");

            var nationalNoExists = await _unitOfWork.Person.AnyAsync(r => r.NationalNo == request.NationalNo);
            if (nationalNoExists)
                throw new ConflictException("National ID Already Exist");

            var phoneExists = await _unitOfWork.Person.AnyAsync(r => r.Phone == request.Phone);
            if (phoneExists)
                throw new ConflictException("Phone Number Already Exist");

            var countryExists = await _unitOfWork.Countries.AnyAsync(r => r.CountryId == request.NationalityCountryId);
            if (!countryExists)
                throw new BadRequestException("Country Not Found");

            var person = new Person
            {
                NationalNo = request.NationalNo,
                FirstName = request.FirstName,
                SecondName = request.SecondName,
                ThirdName = request.ThirdName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Phone = request.Phone,
                NationalityCountryId = request.NationalityCountryId
            };

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _unitOfWork.Person.AddAsync(person);
                await _unitOfWork.CompleteAsync();

                user.PersonId = person.PersonId;
                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Person with NationalNo: {NationalNo} added successfully by User: {UserId}.", request.NationalNo, userId);
            }
            catch (Exception ex) when (ex is not AppException)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while adding a new person. NationalNo: {NationalNo}, UserId: {UserId}.", request.NationalNo, userId);
                throw new BadRequestException($"Something went wrong: {ex.Message}");
            }

            return new ApiResponse<string> { Success = true, Message = "Person Added Successfully" };
        }

        public async Task<ApiResponse<PersonResponseDto>> GetPersonByIDAsync(int personId, int currentUserId, bool isAdmin)
        {
            if (personId <= 0)
                throw new BadRequestException("Invalid Request.");

            string cacheKey = $"person:{personId}";
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                var cachedDto = JsonSerializer.Deserialize<PersonResponseDto>(cachedData);

                if (!isAdmin && cachedDto.UserId != currentUserId)
                    throw new ForbiddenException("Access Denied.");

                return new ApiResponse<PersonResponseDto> { Success = true, Message = "Success from Cache.", Data = cachedDto };
            }

            var person = await _unitOfWork.Person.GetWithTrackingAsync(p => p.PersonId == personId, includes: ["User"]);

            if (person == null || person.IsDeleted == true || person.User?.IsDeleted == true)
                throw new NotFoundException("Person Not Found.");

            if (!isAdmin && person.User?.UserId != currentUserId)
                throw new ForbiddenException("Access Denied.");

            var personDto = new PersonResponseDto
            {
                PersonId = person.PersonId,
                UserId = person.User?.UserId ?? 0,
                NationalNo = person.NationalNo,
                FirstName = person.FirstName,
                SecondName = person.SecondName,
                ThirdName = person.ThirdName,
                LastName = person.LastName,
                DateOfBirth = person.DateOfBirth,
                Gender = person.Gender,
                Phone = person.Phone,
                NationalityCountryId = person.NationalityCountryId
            };

            var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(personDto), cacheOptions);

            return new ApiResponse<PersonResponseDto> { Success = true, Message = "Person Retrieved Successfully.", Data = personDto };
        }

        public async Task<ApiResponse<PersonResponseDto>> GetMyProfileAsync(int currentUserId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(currentUserId);

            if (user == null)
                throw new NotFoundException("User Not Found.");

            if (user.PersonId == null)
                throw new NotFoundException("You don't have a person profile yet.");

            return await GetPersonByIDAsync(user.PersonId.Value, currentUserId, isAdmin: false);
        }

        public async Task<ApiResponse<string>> UpdateMyProfileAsync(int currentUserId, PersonRequestDto request)
        {
            if (currentUserId <= 0 || request == null)
                throw new BadRequestException("Invalid Request.");

            var userWithProfile = await _unitOfWork.Users.GetWithTrackingAsync(u => u.UserId == currentUserId, includes: new[] { "Person" });

            if (userWithProfile == null)
                throw new NotFoundException("Target User Not Found");

            if (userWithProfile.Person == null)
                throw new BadRequestException("Profile Not Found, Please Complete Your Profile First");

            int currentPersonId = userWithProfile.PersonId.Value;

            var nationalNoExists = await _unitOfWork.Person.AnyAsync(r => r.NationalNo == request.NationalNo && r.PersonId != currentPersonId);
            if (nationalNoExists)
                throw new ConflictException("National ID Already Exist For Another Person");

            var phoneExists = await _unitOfWork.Person.AnyAsync(r => r.Phone == request.Phone && r.PersonId != currentPersonId);
            if (phoneExists)
                throw new ConflictException("Phone Number Already Exist For Another Person");

            var countryExists = await _unitOfWork.Countries.AnyAsync(r => r.CountryId == request.NationalityCountryId);
            if (!countryExists)
                throw new BadRequestException("Country Not Found");

            userWithProfile.Person.NationalNo = request.NationalNo;
            userWithProfile.Person.FirstName = request.FirstName;
            userWithProfile.Person.SecondName = request.SecondName;
            userWithProfile.Person.ThirdName = request.ThirdName;
            userWithProfile.Person.LastName = request.LastName;
            userWithProfile.Person.DateOfBirth = request.DateOfBirth;
            userWithProfile.Person.Gender = request.Gender;
            userWithProfile.Person.Phone = request.Phone;
            userWithProfile.Person.NationalityCountryId = request.NationalityCountryId;

            _unitOfWork.Person.Update(userWithProfile.Person);
            await _unitOfWork.CompleteAsync();

            string cacheKey = $"person:{currentPersonId}";
            await _cache.RemoveAsync(cacheKey);

            _logger.LogInformation("User updated their own profile successfully. UserId: {UserId} - {Email}", currentUserId, userWithProfile.Email);

            return new ApiResponse<string> { Success = true, Message = "Person Updated Successfully" };
        }

        public async Task<ApiResponse<string>> UpdatePersonByAdminAsync(int personId, PersonRequestDto request, int currentAdminId)
        {
            if (personId <= 0 || currentAdminId <= 0 || request == null)
                throw new BadRequestException("Invalid Request.");

            var admin = await _unitOfWork.Users.GetByIdAsync(currentAdminId);
            if (admin == null)
                throw new NotFoundException("Admin user not found.");

            var userWithProfile = await _unitOfWork.Users.GetWithTrackingAsync(p => p.PersonId == personId, includes: new[] { "Person" });
            if (userWithProfile == null)
                throw new NotFoundException("Target user not found.");

            if (userWithProfile.IsDeleted == true) 
                throw new BadRequestException("Target person account is deleted.");

            if (userWithProfile.IsEmailVerified == false) 
                throw new BadRequestException("Target person email is unverified.");

            if (userWithProfile.IsActive == false) 
                throw new BadRequestException("Target person account is inactive.");

            if (userWithProfile.Person == null)
                throw new NotFoundException("Profile data not found for this user.");

            if (userWithProfile.Person.IsDeleted)
                throw new BadRequestException("Target profile is deleted in people directory.");

            int currentPersonId = userWithProfile.PersonId.Value;

            var nationalNoExists = await _unitOfWork.Person.AnyAsync(r => r.NationalNo == request.NationalNo && r.PersonId != currentPersonId);
            if (nationalNoExists)
                throw new ConflictException("National ID Already Exists For Another Person");

            var phoneExists = await _unitOfWork.Person.AnyAsync(r => r.Phone == request.Phone && r.PersonId != currentPersonId);
            if (phoneExists)
                throw new ConflictException("Phone Number Already Exists For Another Person");

            var countryExists = await _unitOfWork.Countries.AnyAsync(r => r.CountryId == request.NationalityCountryId);
            if (!countryExists)
                throw new BadRequestException("Country Not Found");

            userWithProfile.Person.NationalNo = request.NationalNo;
            userWithProfile.Person.FirstName = request.FirstName;
            userWithProfile.Person.SecondName = request.SecondName;
            userWithProfile.Person.ThirdName = request.ThirdName;
            userWithProfile.Person.LastName = request.LastName;
            userWithProfile.Person.DateOfBirth = request.DateOfBirth;
            userWithProfile.Person.Gender = request.Gender;
            userWithProfile.Person.Phone = request.Phone;
            userWithProfile.Person.NationalityCountryId = request.NationalityCountryId;

            _unitOfWork.Person.Update(userWithProfile.Person);
            await _unitOfWork.CompleteAsync();

            await _cache.RemoveAsync($"person:{currentPersonId}");

            _logger.LogInformation("Admin (Id: {AdminId}) updated user profile successfully. Target UserId: {UserId} - Email: {Email}",
                currentAdminId, userWithProfile.UserId, userWithProfile.Email);

            return new ApiResponse<string> { Success = true, Message = "Person Updated Successfully By Admin" };
        }

        public async Task<ApiResponse<IEnumerable<PersonResponseDto>>> GetAllPeopleAsync(PagedFilterRequestDto request)
        {
            Expression<Func<Person, bool>>? matchExp = null;

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim();

                matchExp = p => p.FirstName.StartsWith(term) ||
                                p.LastName.StartsWith(term) ||
                                p.Phone.StartsWith(term) ||
                                p.NationalNo.StartsWith(term);
            }

            Expression<Func<Person, object>>? orderByExp = null;

            switch (request.OrderBy?.ToLower())
            {
                case "personid":
                    orderByExp = p => p.PersonId;
                    break;
                case "nationalno":
                    orderByExp = p => p.NationalNo;
                    break;
                case "firstname":
                    orderByExp = p => p.FirstName;
                    break;
                case "secondname":
                    orderByExp = p => p.SecondName;
                    break;
                case "thirdname":
                    orderByExp = p => p.ThirdName;
                    break;
                case "lastname":
                    orderByExp = p => p.LastName;
                    break;
                case "dateofbirth":
                    orderByExp = p => p.DateOfBirth;
                    break;
                case "gender":
                    orderByExp = p => p.Gender;
                    break;
                case "phone":
                    orderByExp = p => p.Phone;
                    break;
                case "nationalitycountryid":
                    orderByExp = p => p.NationalityCountryId;
                    break;
                default:
                    orderByExp = p => p.PersonId;
                    break;
            }

            var direction = request.OrderByDirection?.ToLower().StartsWith("desc") == true ? "desc" : "asc";

            var persons = await _unitOfWork.Person.FindAllAsync(
                match: matchExp,
                Take: request.Limit,
                Skip: request.Page.HasValue ? (request.Page - 1) * request.Limit : null,
                orderBy: orderByExp,
                orderByDirection: direction
            );

            var result = persons.Select(p => new PersonResponseDto
            {
                PersonId = p.PersonId,
                NationalNo = p.NationalNo,
                FirstName = p.FirstName,
                SecondName = p.SecondName,
                ThirdName = p.ThirdName,
                LastName = p.LastName,
                DateOfBirth = p.DateOfBirth,
                Gender = p.Gender,
                Phone = p.Phone,
                NationalityCountryId = p.NationalityCountryId
            });

            return new ApiResponse<IEnumerable<PersonResponseDto>> { Success = true, Message = "People Retrieved Successfully.", Data = result };
        }
    }
}
