using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Common;

namespace TrainingCenter.Core.DTOs.Users
{
    public class GetAllUsersRequestDto : PaginationRequestDto
    {
        [DefaultValue(false)]
        public bool IncludeInfo { get; set; } = false;
    }
}
