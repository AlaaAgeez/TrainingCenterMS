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
    public interface IStudentsRepository : IBaseRepository<Student>
    {
        Task<IEnumerable<Student>> FindAllStudentsWithUsersAsync(
            PagedFilterRequestDto request,
            Expression<Func<Student, bool>> match,
            Expression<Func<Student, object>>? orderBy = null,
            string orderByDirection = "asc"
        );

        Task<Student?> GetStudentByUserIdAsync(int userId);
    }
}
