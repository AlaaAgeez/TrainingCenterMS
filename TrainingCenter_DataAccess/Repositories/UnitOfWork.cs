using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading.Tasks;
using TrainingCenter.Core.Data;
using TrainingCenter.Core.Entities;
using TrainingCenter.Core.Interfaces.Repositories;
using TrainingCenter.DataAccess.Repositories;

namespace TrainingCenter.DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IBaseRepository<Country> Countries { get; private set; }
        public ICourseRepository Courses { get; private set; }
        public IBaseRepository<Enrollment> Enrollments { get; private set; }
        public IInstructorRepository Instructors { get; private set; }
        public IBaseRepository<Person> Person { get; private set; }
        public IBaseRepository<Role> Roles { get; private set; }
        public IBaseRepository<User> Users { get; private set; }
        public IStudentsRepository Students { get; private set; } 

        public UnitOfWork(AppDbContext context)
        {
            _context = context;

            Countries = new BaseRepository<Country>(_context);
            Courses = new CourseRepository(_context);
            Enrollments = new BaseRepository<Enrollment>(_context);
            Instructors = new InstructorRepository(_context);
            Person = new BaseRepository<Person>(_context);
            Roles = new BaseRepository<Role>(_context);
            Users = new BaseRepository<User>(_context);

            Students = new StudentsRepository(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
            => await _context.Database.BeginTransactionAsync();

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}