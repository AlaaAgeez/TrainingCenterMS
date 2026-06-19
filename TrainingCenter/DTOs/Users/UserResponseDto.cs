using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.People;

namespace TrainingCenter.Core.DTOs.Users
{
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public bool? IsEmailVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? LastLoginDate { get; set; }
        public int RoleId { get; set; }

        public PersonResponseDto? Person { get; set; }
    }
}
