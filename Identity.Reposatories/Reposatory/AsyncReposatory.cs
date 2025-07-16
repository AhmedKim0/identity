using Identity.DAL;
using Identity.Domain.Entities;
using Identity.Domain.SharedEntities;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Reposatories.Reposatory
{
    public class EfRepository<TEntity> : IAsyncRepository<TEntity> where TEntity : AuditableEntity
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EfRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TEntity?> GetByIdAsync(int id)
        {
            return await _context.Set<TEntity>()
                .Where(e => !e.IsDeleted && e.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<TEntity>> GetAllAsync()
        {
            return await _context.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .ToListAsync();
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
