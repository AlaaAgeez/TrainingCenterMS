using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.Consts;

namespace TrainingCenter.Core.DTOs.Common
{
    public class PaginationRequestDto
    {
        [DefaultValue(1)]
        public int? Page { get; set; } = PaginationConsts.DefaultPage;

        [DefaultValue(10)]
        public int? Limit { get; set; } = PaginationConsts.DefaultLimit;

        public string? OrderBy { get; set; }

        [DefaultValue("asc")]
        public string? OrderByDirection { get; set; }
    }
}
