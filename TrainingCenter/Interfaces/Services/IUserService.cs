using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.DTOs.Users;

namespace TrainingCenter.Core.Interfaces.Services
{
    public interface IUserService
    {
        Task<ApiResponse<IEnumerable<UserResponseDto>>> GetAllAsync(GetAllUsersRequestDto request);

        Task<ApiResponse<UserResponseDto>> GetUserByIdAsync(int userId);

        Task<ApiResponse<string>> ActivateAccountAsync(int userId);

        Task<ApiResponse<string>> DeactivateAccountAsync(int userId);

        Task<ApiResponse<string>> DeleteAccountAsync(int userId);

        Task<ApiResponse<string>> RestoreAccountAsync(int userId);

        Task<ApiResponse<string>> ChangeUserRoleAsync(int userId, ChangeUserRoleDto request);

        Task<ApiResponse<string>> ChangePasswordAsync(int userId, ChangePasswordDto request);

        Task<ApiResponse<string>> ChangeEmailAsync(ChangeEmailDto request, int userId);
    }
}
