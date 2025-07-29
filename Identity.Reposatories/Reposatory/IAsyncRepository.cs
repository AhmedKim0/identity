using Identity.Domain.Entities;
using Identity.Domain.SharedEntities;

using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;

namespace Identity.Application.Reposatory
{
    public interface IAsyncRepository<TEntity> where TEntity : BaseEntity
    {
        Task<TEntity?> GetByIdAsync(int id);
        Task<List<TEntity>> GetAllAsync();
        Task AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TEntity entity); 
        Task<int> SaveChangesAsync();
        DbSet<TEntity> Dbset();
        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

    }

}
