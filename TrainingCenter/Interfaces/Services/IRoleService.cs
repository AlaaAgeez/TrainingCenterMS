using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.DTOs.Roles;

namespace TrainingCenter.Core.Interfaces.Services
{
    public interface IRoleService
    {
        Task<ApiResponse<IEnumerable<RoleResponseDto>>> GetAllRolesAsync();

        Task<ApiResponse<RoleResponseDto>> GetRoleByIdAsync(byte roleId);

        Task<ApiResponse<string>> CreateRoleAsync(CreateRoleRequestDto request, string adminEmail);

        Task<ApiResponse<string>> UpdateRoleNameAsync(UpdateRoleRequestDto request, string adminEmail);

        Task<ApiResponse<string>> DeleteRoleAsync(byte roleId, string adminEmail);
    }
}
