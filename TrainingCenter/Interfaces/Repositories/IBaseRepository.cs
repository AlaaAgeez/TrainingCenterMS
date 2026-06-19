using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TrainingCenter.Core.Consts;

namespace TrainingCenter.Core.Interfaces.Repositories
{
    public interface IBaseRepository<T> where T : class
    {
        T GetByid(int id);

        Task<T> GetByIdAsync(int id);

        IEnumerable<T> GetAll();

        Task <IEnumerable<T>> GetAllAsync();

        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        Task<T> FindAsync(Expression<Func<T, bool>> match, string[]? includes = null);

        Task<T?> GetReadOnlyAsync(Expression<Func<T, bool>> predicate, string[]? includes = null);

        Task<T?> GetWithTrackingAsync(Expression<Func<T, bool>> predicate, string[]? includes = null);

        IEnumerable<T> FindAll(Expression<Func<T, bool>> match, int Take, int Skip);

        Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>>? match = null, string[]? includes = null,
            int? Take = null, int? Skip = null, Expression<Func<T, object>>? orderBy = null, 
            string orderByDirection = OrderBy.Ascending);

        Task<T> AddAsync(T entity);

        IEnumerable <T> AddRange(IEnumerable<T> entities);

        T Update(T entity);

        Task<bool> DeleteAsync(T entity);

        Task<bool> AnyAsync(Expression<Func<T, bool>> match);
    }
}
