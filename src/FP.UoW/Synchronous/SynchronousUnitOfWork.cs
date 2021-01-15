using System;
using System.Data.Common;

namespace FP.UoW.Synchronous
{
    /// <summary>
    /// Synchronous implementation of an Unit of Work.
    /// Saves resources for all those ADO .NET Providers that do not support natively the Task-based asynchronous interface.
    /// </summary>
    public sealed class SynchronousUnitOfWork : ISynchronousUnitOfWork, IDisposable
    {
        private readonly IDatabaseConnectionFactory connectionFactory;

        private readonly UnitOfWorkOptionsProviderFunc unitOfWorkOptionsProvider;

        public SynchronousUnitOfWork(IDatabaseConnectionFactory connectionFactory, UnitOfWorkOptionsProviderFunc unitOfWorkOptionsProvider)
        {
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            this.unitOfWorkOptionsProvider = unitOfWorkOptionsProvider ?? throw new ArgumentNullException(nameof(unitOfWorkOptionsProvider));
        }

        public SynchronousUnitOfWork(IDatabaseConnectionFactory connectionFactory, UnitOfWorkOptions unitOfWorkOptions)
            : this(connectionFactory, () => unitOfWorkOptions)
        {
        }

        public SynchronousUnitOfWork(IDatabaseConnectionFactory connectionFactory)
            : this(connectionFactory, UnitOfWorkOptions.Default)
        {
        }

        private UnitOfWorkOptions UnitOfWorkOptions => unitOfWorkOptionsProvider();

        /// <inheritdoc />
        public DbConnection Connection { get; private set; }

        /// <inheritdoc />
        public DbTransaction Transaction { get; private set; }

        public void Dispose()
        {
            Transaction?.Dispose();
            Connection?.Dispose();

            Transaction = null;
            Connection = null;
        }

        /// <inheritdoc />
        public void OpenConnection()
        {
            if (Connection != null)
            {
                if (UnitOfWorkOptions.ThrowOnMultipleConnectionsAttempts)
                {
                    throw new InvalidOperationException("There is already a database connection open, you must close it before opening another one");
                }

                return;
            }

            var newConnection = connectionFactory.MakeNew();

            if (newConnection is null)
            {
                throw new InvalidOperationException("No DbConnection instance was created, implementation returned null");
            }

            newConnection.Open();

            Connection = newConnection;
        }

        /// <inheritdoc />
        public void CloseConnection()
        {
            if (Connection is null)
            {
                return;
            }

            if (Transaction != null)
            {
                throw new InvalidOperationException("There is a transaction running, you cannot close the database connection until you don't decide what to do with the transaction");
            }

            Connection.Close();
            Connection.Dispose();

            Connection = null;
        }

        /// <inheritdoc />
        public void BeginTransaction()
        {
            if (Transaction != null)
            {
                if (UnitOfWorkOptions.ThrowOnMultipleTransactionsAttempts)
                {
                    throw new InvalidOperationException("There is a transaction already running, you cannot start a new transaction until you don't decide what to do with the transaction");
                }

                return;
            }

            if (Connection is null)
            {
                OpenConnection();
            }

            var newTransaction = Connection.BeginTransaction();

            Transaction = newTransaction;
        }

        /// <inheritdoc />
        public void CommitTransaction()
        {
            if (Transaction is null)
            {
                throw new InvalidOperationException("You must begin a transaction before committing it");
            }

            Transaction.Commit();
            Transaction.Dispose();

            Transaction = null;

            CloseConnection();
        }

        /// <inheritdoc />
        public void RollbackTransaction()
        {
            if (Transaction is null)
            {
                throw new InvalidOperationException("You must begin a transaction before rolling it back");
            }

            Transaction.Rollback();
            Transaction.Dispose();

            Transaction = null;

            CloseConnection();
        }
    }
}
