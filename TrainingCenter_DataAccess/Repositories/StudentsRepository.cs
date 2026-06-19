using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.Data;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.Entities;
using TrainingCenter.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TrainingCenter.DataAccess.Repositories
{
    public class StudentsRepository : BaseRepository<Student>, IStudentsRepository
    {
        private readonly AppDbContext _context;

        public StudentsRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Student>> FindAllStudentsWithUsersAsync(
            PagedFilterRequestDto request,
            Expression<Func<Student, bool>> match,
            Expression<Func<Student, object>>? orderBy = null,
            string orderByDirection = "asc")
        {
            IQueryable<Student> query = _context.Students
                .Include(s => s.User)
                    .ThenInclude(u => u.Person);

            query = query.Where(match);

            if (orderBy != null)
            {
                query = orderByDirection.ToLower() == "desc"
                    ? query.OrderByDescending(orderBy)
                    : query.OrderBy(orderBy);
            }

            return await query
                .Skip(((request.Page ?? 1) - 1) * (request.Limit ?? 10)) 
                .Take(request.Limit ?? 10)
                .ToListAsync();
        }

        public async Task<Student?> GetStudentByUserIdAsync(int userId)
        {
            return await _context.Students
                .Include(s => s.User)
                    .ThenInclude(u => u.Person)
                .FirstOrDefaultAsync(s => s.UserId == userId && s.IsDeleted == false);
        }
    }
}
