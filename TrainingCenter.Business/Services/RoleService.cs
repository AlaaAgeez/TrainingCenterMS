using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.DTOs.Roles;
using TrainingCenter.Core.Entities;
using TrainingCenter.Core.Exceptions;
using TrainingCenter.Core.Interfaces.Repositories;
using TrainingCenter.Core.Interfaces.Services;

namespace TrainingCenter.Business.Servicesش
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RoleService> _logger;

        public RoleService(IUnitOfWork unitOfWork, ILogger<RoleService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<RoleResponseDto>>> GetAllRolesAsync()
        {
            var roles = await _unitOfWork.Roles.GetAllAsync();

            if (!roles.Any())
                return new ApiResponse<IEnumerable<RoleResponseDto>>
                {
                    Success = true,
                    Message = "No roles found.",
                    Data = Enumerable.Empty<RoleResponseDto>()
                };

            var result = roles.Select(r => new RoleResponseDto { RoleId = r.RoleId, RoleName = r.RoleName }).ToList();
            return new ApiResponse<IEnumerable<RoleResponseDto>> { Success = true, Message = "Roles retrieved successfully.", Data = result };
        }

        public async Task<ApiResponse<RoleResponseDto>> GetRoleByIdAsync(byte roleId)
        {
            if (roleId <= 0) throw new BadRequestException("Invalid ID.");

            var role = await _unitOfWork.Roles.FindAsync(r => r.RoleId == roleId);
            if (role == null) throw new NotFoundException("Role not found.");

            return new ApiResponse<RoleResponseDto> { Success = true, Message = "Role retrieved successfully.", Data = new RoleResponseDto { RoleId = role.RoleId, RoleName = role.RoleName } };
        }

        public async Task<ApiResponse<string>> CreateRoleAsync(CreateRoleRequestDto request, string adminEmail)
        {
            if (await _unitOfWork.Roles.ExistsAsync(r => r.RoleName.ToLower() == request.RoleName.ToLower()))
                throw new ConflictException("Role already exists.");

            await _unitOfWork.Roles.AddAsync(new Role { RoleName = request.RoleName });
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Admin {AdminEmail} created a new role {RoleName}.", adminEmail, request.RoleName);
            return new ApiResponse<string> { Success = true, Message = "Role created successfully." };
        }

        public async Task<ApiResponse<string>> UpdateRoleNameAsync(UpdateRoleRequestDto request, string adminEmail)
        {
            if (request.RoleId <= 0) throw new BadRequestException("Invalid ID.");

            var role = await _unitOfWork.Roles.FindAsync(r => r.RoleId == request.RoleId);
            if (role == null) throw new NotFoundException("Role not found.");

            if (await _unitOfWork.Roles.ExistsAsync(r => r.RoleName.ToLower() == request.RoleName.ToLower() && r.RoleId != request.RoleId))
                throw new ConflictException("Role name already exists.");

            var oldRoleName = role.RoleName;
            role.RoleName = request.RoleName;
            role.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Roles.Update(role);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Admin {AdminEmail} updated role {RoleId} from {OldRoleName} to {NewRoleName}.", adminEmail, request.RoleId, oldRoleName, request.RoleName);
            return new ApiResponse<string> { Success = true, Message = "Role updated successfully." };
        }

        public async Task<ApiResponse<string>> DeleteRoleAsync(byte roleId, string adminEmail)
        {
            if (roleId <= 0) throw new BadRequestException("Invalid ID.");

            var role = await _unitOfWork.Roles.FindAsync(r => r.RoleId == roleId);
            if (role == null) throw new NotFoundException("Role not found.");

            if (await _unitOfWork.Users.ExistsAsync(u => u.RoleId == roleId))
                throw new BadRequestException("Cannot delete role, it is assigned to users.");

            await _unitOfWork.Roles.DeleteAsync(role);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Admin {AdminEmail} deleted role {RoleId} - {RoleName}.", adminEmail, roleId, role.RoleName);
            return new ApiResponse<string> { Success = true, Message = "Role deleted successfully." };
        }
    }
}