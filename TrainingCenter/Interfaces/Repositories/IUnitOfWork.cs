using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading.Tasks;
using TrainingCenter.Core.Entities;

namespace TrainingCenter.Core.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IBaseRepository<Country> Countries { get; }

        ICourseRepository Courses { get; }

        IBaseRepository<Enrollment> Enrollments { get; }
        IInstructorRepository Instructors { get; }
        IBaseRepository<Person> Person { get; }
        IBaseRepository<Role> Roles { get; }
        IStudentsRepository Students { get; }
        IBaseRepository<User> Users { get; }

        Task<int> CompleteAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}