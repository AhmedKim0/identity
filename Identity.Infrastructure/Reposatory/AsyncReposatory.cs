using Identity.DAL;
using Identity.Domain.Entities;
using Identity.Domain.IReposatory;
using Identity.Domain.SharedEntities;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Reposatory
{
    public class AsyncReposatory<TEntity> : IAsyncRepository<TEntity> where TEntity : BaseEntity
    {
        private readonly AppDbContext _context;

        public AsyncReposatory(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TEntity?> GetByIdAsync(int id)
        {
            return await _context.Set<TEntity>().AsNoTracking()
                .Where(e => !e.IsDeleted && e.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<TEntity>> GetAllAsync()
        {
            return await _context.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .ToListAsync();
        }
        public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _context.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(predicate);
        }

        public DbSet<TEntity> Dbset()
        {
           return _context.Set<TEntity>();
        }
        public async Task AddAsync(TEntity entity)
        {


            await _context.Set<TEntity>().AddAsync(entity);
        }

        public async Task UpdateAsync(TEntity entity)
        {


            _context.Set<TEntity>().Update(entity);
            await Task.CompletedTask; // for async signature
        }

        public async Task DeleteAsync(TEntity entity)
        {
            entity.IsDeleted = true;
             _context.Set<TEntity>().Remove(entity);
            await Task.CompletedTask; // for async signature
        }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }


    }

}
