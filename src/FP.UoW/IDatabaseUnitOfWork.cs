using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace FP.UoW
{
    /// <summary>
    /// Exposes methods to interact with an Unit of Work, allowing to Begin, Rollback or Commit an Unit of Work
    /// </summary>
    public interface IDatabaseUnitOfWork
    {
        Task BeginAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default);

        Task RollbackAsync(CancellationToken cancellationToken = default);

        Task CommitAsync(CancellationToken cancellationToken = default);
    }
}
