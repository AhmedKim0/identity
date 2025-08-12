using System.Data;

namespace Identity.Application.UOW
{

    public interface IUnitOfWork : IAsyncDisposable, IDisposable
    {
        Task BeginTransactionAsync(IsolationLevel isolationLevel);
        Task BeginTransactionAsync();

        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task<int> SaveChangesAsync();
    }


}
