using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IBaseRepositories<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();

        Task<T?> GetByIdAsync(int id);

        Task<T> AddAsync(T entity);

        void UpdateAsync(T entity);

        void Delete(T entity);

        Task SaveChangesAsync();

        Task<IEnumerable<T>> GetAllWithIncludeAsync(params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> GetAllWithIncludeAndFilterAsync(
     Expression<Func<T, bool>> filter,
     params Expression<Func<T, object>>[] includes);

        Task<IDisposable> BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task<TResult> ExecuteRawSqlScalarAsync<TResult>(string sql, params object[] parameters);

    }
}
