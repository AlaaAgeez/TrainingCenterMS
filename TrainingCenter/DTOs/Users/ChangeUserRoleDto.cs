using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingCenter.Core.DTOs.Users
{
    public class ChangeUserRoleDto
    {
        public byte NewRoleId { get; set; }
    }
}
