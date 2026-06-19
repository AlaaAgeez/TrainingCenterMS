using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.Entities;

namespace TrainingCenter.Core.Interfaces.Repositories
{
    public interface ICourseRepository : IBaseRepository<Course>
    {
        Task<IEnumerable<Course>> FindAllCoursesWithInstructorAsync(
            PagedFilterRequestDto request,
            Expression<Func<Course, bool>> match,
            Expression<Func<Course, object>>? orderBy = null,
            string orderByDirection = "asc");
    }
}
