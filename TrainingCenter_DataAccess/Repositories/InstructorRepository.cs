using Microsoft.EntityFrameworkCore;
using TrainingCenter.Core.Entities;
using TrainingCenter.Core.DTOs.Common;
using TrainingCenter.Core.DTOs.Instructors;
using TrainingCenter.Core.Interfaces.Repositories;
using TrainingCenter.Core.Data;

namespace TrainingCenter.DataAccess.Repositories
{
    public class InstructorRepository : BaseRepository<Instructor>, IInstructorRepository
    {
        private readonly AppDbContext _context;

        public InstructorRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InstructorResponseDto>> GetPagedInstructorsAsync(PagedFilterRequestDto request)
        {
            var query = _context.Instructors.Where(i => i.IsDeleted == false && i.User.IsDeleted == false).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim();
                query = query.Where(i =>
                    i.InstructorId.ToString() == searchTerm ||
                    i.User.Email.Contains(searchTerm) ||
                    i.User.Person.FirstName.Contains(searchTerm) ||
                    i.User.Person.LastName.Contains(searchTerm) ||
                    i.User.Person.Phone.Contains(searchTerm));
            }

            var resultQuery = query.Select(i => new InstructorResponseDto
            {
                InstructorId = i.InstructorId,
                Salary = i.Salary,
                HireDate = i.HireDate,
                CreatedAt = i.CreatedAt,
                Email = i.User.Email,
                IsActive = i.User.IsActive,
                IsEmailVerified = i.User.IsEmailVerified ?? false,
                FullName = (i.User.Person.FirstName + " " +
                            (i.User.Person.SecondName != null ? i.User.Person.SecondName + " " : "") +
                            (i.User.Person.ThirdName != null ? i.User.Person.ThirdName + " " : "") +
                            i.User.Person.LastName).Trim(),
                PhoneNumber = i.User.Person.Phone
            });

            int page = request.Page ?? 1;
            int limit = request.Limit ?? 10;

            return await resultQuery
                .Skip((page - 1) * limit)
                .Take(limit)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<InstructorResponseDto?> GetInstructorByIdAsync(int instructorId)
        {
            return await _context.Instructors
                    .Where(i => i.InstructorId == instructorId && i.User.IsDeleted == false)
                    .Select(i => new InstructorResponseDto
                    {
                        InstructorId = i.InstructorId,
                        Salary = i.Salary,
                        HireDate = i.HireDate,
                        CreatedAt = i.CreatedAt,
                        Email = i.User.Email,
                        IsActive = i.User.IsActive,
                        IsEmailVerified = i.User.IsEmailVerified ?? false, 
                        FullName = (i.User.Person.FirstName + " " +
                                    (i.User.Person.SecondName != null ? i.User.Person.SecondName + " " : "") +
                                    (i.User.Person.ThirdName != null ? i.User.Person.ThirdName + " " : "") +
                                    i.User.Person.LastName).Trim(),
                        PhoneNumber = i.User.Person.Phone
                    })
                    .FirstOrDefaultAsync();
        }
    }
}