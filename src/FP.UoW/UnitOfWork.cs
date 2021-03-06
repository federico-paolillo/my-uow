﻿using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace FP.UoW
{
    /// <summary>
    /// Implementation of an Unit of Work.
    /// </summary>
    public sealed class UnitOfWork : IUnitOfWork, IDisposable, IAsyncDisposable
    {
        private readonly IDatabaseConnectionFactory connectionFactory;

        private readonly UnitOfWorkOptionsProviderFunc unitOfWorkOptionsProvider;

        public UnitOfWork(IDatabaseConnectionFactory connectionFactory, UnitOfWorkOptionsProviderFunc unitOfWorkOptionsProvider)
        {
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            this.unitOfWorkOptionsProvider = unitOfWorkOptionsProvider ?? throw new ArgumentNullException(nameof(unitOfWorkOptionsProvider));
        }

        public UnitOfWork(IDatabaseConnectionFactory connectionFactory, UnitOfWorkOptions unitOfWorkOptions)
            : this(connectionFactory, () => unitOfWorkOptions)
        {
        }

        public UnitOfWork(IDatabaseConnectionFactory connectionFactory)
            : this(connectionFactory, UnitOfWorkOptions.Default)
        {
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Transaction?.Dispose();
                Connection?.Dispose();

                Transaction = null;
                Connection = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (Transaction is not null)
            {
                await Transaction.DisposeAsync()
                    .ConfigureAwait(continueOnCapturedContext: false);
            }

            if (Connection is not null)
            {
                await Connection.DisposeAsync()
                    .ConfigureAwait(continueOnCapturedContext: false);
            }

            Connection = null;
            Transaction = null;

            Dispose(false);

#pragma warning disable CA1816

            // This is a DisposeAsync method, therefore SuppressFinalize call is appropriate in this context
            GC.SuppressFinalize(this);

#pragma warning restore CA1816 
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        public UnitOfWorkOptions Options => unitOfWorkOptionsProvider() ?? UnitOfWorkOptions.Default;

        /// <inheritdoc />
        public DbConnection Connection { get; private set; }

        /// <inheritdoc />
        public DbTransaction Transaction { get; private set; }

        /// <inheritdoc />
        public async Task OpenConnectionAsync(CancellationToken cancellationToken = default)
        {
            if (Connection != null)
            {
                if (Options.ThrowOnMultipleConnectionsAttempts)
                {
                    throw new InvalidOperationException("There is already a database connection open, you must close it before opening another one");
                }

                return;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var newConnection = await connectionFactory.MakeNewAsync(cancellationToken)
                .ConfigureAwait(false);

            if (newConnection is null)
            {
                throw new InvalidOperationException("No DbConnection instance was created, implementation returned null");
            }

            await newConnection.OpenAsync(cancellationToken)
                .ConfigureAwait(false);

            Connection = newConnection;
        }

        /// <inheritdoc />
        public async Task CloseConnectionAsync(CancellationToken cancellationToken = default)
        {
            if (Connection is null)
            {
                return;
            }

            if (Transaction != null)
            {
                throw new InvalidOperationException("There is a transaction running, you cannot close the database connection until you don't decide what to do with the transaction");
            }

            cancellationToken.ThrowIfCancellationRequested();

            await Connection.CloseAsync()
                .ConfigureAwait(false);

            await Connection.DisposeAsync()
                .ConfigureAwait(false);

            Connection = null;
        }

        /// <inheritdoc />
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (Transaction != null)
            {
                if (Options.ThrowOnMultipleTransactionsAttempts)
                {
                    throw new InvalidOperationException("There is a transaction already running, you cannot start a new transaction until you don't decide what to do with the transaction");
                }

                return;
            }

            if (Connection is null)
            {
                await OpenConnectionAsync(cancellationToken)
                    .ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();

            var newTransaction = await Connection.BeginTransactionAsync(cancellationToken)
                .ConfigureAwait(false);

            Transaction = newTransaction;
        }

        /// <inheritdoc />
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (Transaction is null)
            {
                throw new InvalidOperationException("You must begin a transaction before committing it");
            }

            cancellationToken.ThrowIfCancellationRequested();

            await Transaction.CommitAsync(cancellationToken)
                .ConfigureAwait(false);

            await Transaction.DisposeAsync()
                .ConfigureAwait(false);

            Transaction = null;

            await CloseConnectionAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (Transaction is null)
            {
                throw new InvalidOperationException("You must begin a transaction before rolling it back");
            }

            await Transaction.RollbackAsync(cancellationToken)
                .ConfigureAwait(false);

            await Transaction.DisposeAsync()
                .ConfigureAwait(false);

            Transaction = null;

            await CloseConnectionAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}