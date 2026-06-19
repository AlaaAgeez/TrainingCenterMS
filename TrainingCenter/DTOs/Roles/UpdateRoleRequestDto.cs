using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingCenter.Core.DTOs.Roles
{
    public class UpdateRoleRequestDto
    {
        public byte RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
