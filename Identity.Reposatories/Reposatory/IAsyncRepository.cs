using Identity.Domain.Entities;
using Identity.Domain.SharedEntities;

namespace Identity.Reposatories.Reposatory
{
    public interface IAsyncRepository<TEntity> where TEntity : AuditableEntity
    {
        Task<TEntity?> GetByIdAsync(int id);
        Task<List<TEntity>> GetAllAsync();
        Task AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TEntity entity); 
        Task<int> SaveChangesAsync();
    }

}
