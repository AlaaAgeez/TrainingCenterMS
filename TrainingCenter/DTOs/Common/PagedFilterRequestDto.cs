using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingCenter.Core.DTOs.Common
{
    public class PagedFilterRequestDto : PaginationRequestDto
    {
        public string? SearchTerm { get; set; }
    }
}
