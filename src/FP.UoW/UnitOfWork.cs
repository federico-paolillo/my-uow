using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace FP.UoW
{
    /// <summary>
    /// Implementation of an Unit of Work
    /// </summary>
    public sealed class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly IDatabaseConnectionFactory connectionFactory;

        /// <inheritdoc />
        public DbConnection Connection { get; private set; }

        /// <inheritdoc />
        public DbTransaction Transaction { get; private set; }

        public UnitOfWork(IDatabaseConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        /// <inheritdoc />
        public async Task OpenConnectionAsync(CancellationToken cancellationToken = default)
        {
            if (Connection != null)
                throw new InvalidOperationException(
                    "There is already a database connection open, you must close it before opening another one");

            cancellationToken.ThrowIfCancellationRequested();

            var newConnection = await connectionFactory.MakeNewAsync(cancellationToken)
                .ConfigureAwait(continueOnCapturedContext: false);

            if (newConnection is null)
                throw new InvalidOperationException(
                    "No DbConnection instance was created, implementation returned null");

            await newConnection.OpenAsync(cancellationToken)
                .ConfigureAwait(continueOnCapturedContext: false);

            Connection = newConnection;
        }

        /// <inheritdoc />
        public async Task CloseConnectionAsync(CancellationToken cancellationToken = default)
        {
            if (Connection is null) return;

            if (Transaction != null)
                throw new InvalidOperationException(
                    "There is a transaction running, you cannot close the database connection until you don't decide what to do with the transaction");

            cancellationToken.ThrowIfCancellationRequested();

            await Connection.CloseAsync()
                .ConfigureAwait(continueOnCapturedContext: false);

            await Connection.DisposeAsync()
                .ConfigureAwait(continueOnCapturedContext: false);

            Connection = null;
        }

        /// <inheritdoc />
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (Transaction != null)
                throw new InvalidOperationException(
                    "There is a transaction already running, you cannot start a new transaction until you don't decide what to do with the transaction");

            if (Connection is null)
            {
                await OpenConnectionAsync(cancellationToken)
                    .ConfigureAwait(continueOnCapturedContext: false);
            }

            cancellationToken.ThrowIfCancellationRequested();

            var newTransaction = await Connection.BeginTransactionAsync(cancellationToken)
                .ConfigureAwait(continueOnCapturedContext: false);

            Transaction = newTransaction;
        }

        /// <inheritdoc />
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (Transaction is null)
                throw new InvalidOperationException("You must begin a transaction before committing it");

            cancellationToken.ThrowIfCancellationRequested();

            await Transaction.CommitAsync(cancellationToken)
                .ConfigureAwait(continueOnCapturedContext: false);

            await Transaction.DisposeAsync()
                .ConfigureAwait(continueOnCapturedContext: false);

            Transaction = null;

            await CloseConnectionAsync(cancellationToken)
                .ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <inheritdoc />
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (Transaction is null)
                throw new InvalidOperationException("You must begin a transaction before rolling it back");

            cancellationToken.ThrowIfCancellationRequested();

            await Transaction.RollbackAsync(cancellationToken)
                .ConfigureAwait(continueOnCapturedContext: false);

            await Transaction.DisposeAsync()
                .ConfigureAwait(continueOnCapturedContext: false);

            Transaction = null;

            await CloseConnectionAsync(cancellationToken)
                .ConfigureAwait(continueOnCapturedContext: false);
        }

        public void Dispose()
        {
            Transaction?.Dispose();
            Connection?.Dispose();

            Transaction = null;
            Connection = null;
        }
    }
}