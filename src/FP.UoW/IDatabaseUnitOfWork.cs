using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace FP.UoW
{
    /// <summary>
    /// Exposes methods to interact with an Unit of Work, allowing to Begin, Rollback or Commit an Unit of Work
    /// </summary>
    public interface IDatabaseUnitOfWork : IDatabaseSession
    {
        /// <summary>
        /// Opens a connection to the database.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
        Task OpenConnectionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Close the connection to the database.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
        /// <returns></returns>
        Task CloseConnectionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Begin a transaction using the open connection with the <see cref="IsolationLevel"/> specified.
        /// </summary>
        /// <param name="isolationLevel">Specifies the transaction locking behavior for the transaction</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
        Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default);

        /// <summary>
        /// Rollsback the current transaction, removing any changes from the database
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Commits the current transaction, persisting changes to the database
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    }
}
