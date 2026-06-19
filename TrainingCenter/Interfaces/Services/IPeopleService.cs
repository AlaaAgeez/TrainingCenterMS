using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.DTOs.People;
using TrainingCenter.Core.DTOs.Users;

namespace TrainingCenter.Core.Interfaces.Services
{
    public interface IPeopleService
    {
        Task<ApiResponse<string>> AddNewPersonasync(PersonRequestDto request, int userId);

        Task<ApiResponse<PersonResponseDto>> GetMyProfileAsync(int userId);

        Task<ApiResponse<string>> UpdateMyProfileAsync(int currentUserId, PersonRequestDto request);

        Task<ApiResponse<string>> UpdatePersonByAdminAsync(int personId, PersonRequestDto request, int currentAdminId);

        Task<ApiResponse<IEnumerable<PersonResponseDto>>> GetAllPeopleAsync(PagedFilterRequestDto request);

        Task<ApiResponse<PersonResponseDto>> GetPersonByIDAsync(int personId, int currentUserId, bool isAdmin);
    }
}
