using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingCenter.Core.DTOs.Users
{
    public class ChangeEmailDto
    {
        public string NewEmail { get; set; }

        public string CurrentPassword { get; set; }
    }
}
