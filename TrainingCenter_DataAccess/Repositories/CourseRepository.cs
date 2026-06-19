using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TrainingCenter.Core.Data;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.Entities;
using TrainingCenter.Core.Interfaces.Repositories;

namespace TrainingCenter.DataAccess.Repositories
{
    public class CourseRepository : BaseRepository<Course>, ICourseRepository
    {
        private readonly AppDbContext _context;

        public CourseRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Course>> FindAllCoursesWithInstructorAsync(
            PagedFilterRequestDto request,
            Expression<Func<Course, bool>> match,
            Expression<Func<Course, object>>? orderBy = null,
            string orderByDirection = "asc")
        {
            var query = _context.Courses.AsNoTracking().Include(c => c.Instructor).ThenInclude(i => i.User).ThenInclude(u => u.Person).Where(match);

            if (orderBy != null)
                query = orderByDirection == "desc"? query.OrderByDescending(orderBy): query.OrderBy(orderBy);

            int page = request.Page ?? 1;
            int limit = request.Limit ?? 10;
            int skip = (page - 1) * limit;

            query = query.Skip(skip).Take(limit);

            return await query.ToListAsync();
        }
    }
}