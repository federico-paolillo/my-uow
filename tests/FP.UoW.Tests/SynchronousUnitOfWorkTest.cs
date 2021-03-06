﻿using FP.UoW.SQLite;
using FP.UoW.Synchronous;

using Moq;

using NUnit.Framework;

using System.Threading;

namespace FP.UoW.Tests
{
    public sealed class SynchronousUnitOfWorkTest
    {
        [Test]
        public void A_Connection_can_t_be_open_twice()
        {
            var sqliteDatabaseConnectionString = new SQLiteDatabaseConnectionString("Data Source = whatever.db");
            var sqliteDatabaseConnectionFactory = new SQLiteDatabaseConnectionFactory(sqliteDatabaseConnectionString);

            using var unitOfWork = new SynchronousUnitOfWork(sqliteDatabaseConnectionFactory);

            unitOfWork.OpenConnection();

            void OpenConnectionAgain()
            {
                unitOfWork.OpenConnection();
            }

            Assert.That(OpenConnectionAgain, Throws.InvalidOperationException);
        }

        [Test]
        public void A_Transaction_can_t_begin_twice()
        {
            var sqliteDatabaseConnectionString = new SQLiteDatabaseConnectionString("Data Source = whatever.db");
            var sqliteDatabaseConnectionFactory = new SQLiteDatabaseConnectionFactory(sqliteDatabaseConnectionString);

            using var unitOfWork = new SynchronousUnitOfWork(sqliteDatabaseConnectionFactory);

            unitOfWork.BeginTransaction();

            void BeginTransactionAgain()
            {
                unitOfWork.BeginTransaction();
            }

            Assert.That(BeginTransactionAgain, Throws.InvalidOperationException);
        }

        [Test]
        public void Unit_of_Work_complains_if_Connection_Factory_Returns_null()
        {
            var brokenConnectionFactoryMock = new Mock<IDatabaseConnectionFactory>();

            brokenConnectionFactoryMock.Setup(m => m.MakeNewAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(value: null);

            using var unitOfWork = new SynchronousUnitOfWork(brokenConnectionFactoryMock.Object);

            void TryOpenConnection()
            {
                unitOfWork.OpenConnection();
            }

            Assert.That(TryOpenConnection, Throws.InvalidOperationException);
        }

        [Test]
        public void Connection_can_t_be_closed_without_commiting_or_rolling_back_the_transaction()
        {
            var sqliteDatabaseConnectionString = new SQLiteDatabaseConnectionString("Data Source = whatever.db");
            var sqliteDatabaseConnectionFactory = new SQLiteDatabaseConnectionFactory(sqliteDatabaseConnectionString);

            using var unitOfWork = new SynchronousUnitOfWork(sqliteDatabaseConnectionFactory);

            unitOfWork.BeginTransaction();

            void TryCloseConnection()
            {
                unitOfWork.CloseConnection();
            }

            Assert.That(TryCloseConnection, Throws.InvalidOperationException);
        }

        [Test]
        public void Transaction_can_t_be_rolled_back_without_beginning_it()
        {
            var sqliteDatabaseConnectionString = new SQLiteDatabaseConnectionString("Data Source = whatever.db");
            var sqliteDatabaseConnectionFactory = new SQLiteDatabaseConnectionFactory(sqliteDatabaseConnectionString);

            using var unitOfWork = new SynchronousUnitOfWork(sqliteDatabaseConnectionFactory);

            void TryRollbackTransaction()
            {
                unitOfWork.RollbackTransaction();
            }

            Assert.That(TryRollbackTransaction, Throws.InvalidOperationException);
        }

        [Test]
        public void Transaction_can_t_be_commited_without_beginning_it()
        {
            var sqliteDatabaseConnectionString = new SQLiteDatabaseConnectionString("Data Source = whatever.db");
            var sqliteDatabaseConnectionFactory = new SQLiteDatabaseConnectionFactory(sqliteDatabaseConnectionString);

            using var unitOfWork = new SynchronousUnitOfWork(sqliteDatabaseConnectionFactory);

            void TryCommitTransaction()
            {
                unitOfWork.CommitTransaction();
            }

            Assert.That(TryCommitTransaction, Throws.InvalidOperationException);
        }

        [Test]
        public void Closing_Connection_that_are_not_open_does_nothing()
        {
            var sqliteDatabaseConnectionString = new SQLiteDatabaseConnectionString("Data Source = whatever.db");
            var sqliteDatabaseConnectionFactory = new SQLiteDatabaseConnectionFactory(sqliteDatabaseConnectionString);

            using var unitOfWork = new SynchronousUnitOfWork(sqliteDatabaseConnectionFactory);

            void TryCloseConnection()
            {
                unitOfWork.CloseConnection();
            }

            Assert.DoesNotThrow(TryCloseConnection);
        }

        [Test]
        public void Multiple_transaction_errors_are_ignored_when_the_appropriate_option_is_specified()
        {
            var sqliteDatabaseConnectionString = new SQLiteDatabaseConnectionString("Data Source = whatever.db");
            var sqliteDatabaseConnectionFactory = new SQLiteDatabaseConnectionFactory(sqliteDatabaseConnectionString);

            var options = new UnitOfWorkOptions
            {
                ThrowOnMultipleTransactionsAttempts = false,
                ThrowOnMultipleConnectionsAttempts = true
            };

            using var unitOfWork = new SynchronousUnitOfWork(sqliteDatabaseConnectionFactory, options);

            unitOfWork.BeginTransaction();

            void BeginTransactionAgain()
            {
                unitOfWork.BeginTransaction();
            }

            Assert.DoesNotThrow(BeginTransactionAgain);
        }

        [Test]
        public void Multiple_connections_errors_are_ignored_when_the_appropriate_option_is_specified()
        {
            var sqliteDatabaseConnectionString = new SQLiteDatabaseConnectionString("Data Source = whatever.db");
            var sqliteDatabaseConnectionFactory = new SQLiteDatabaseConnectionFactory(sqliteDatabaseConnectionString);

            var options = new UnitOfWorkOptions
            {
                ThrowOnMultipleTransactionsAttempts = true,
                ThrowOnMultipleConnectionsAttempts = false
            };

            using var unitOfWork = new SynchronousUnitOfWork(sqliteDatabaseConnectionFactory, options);

            unitOfWork.OpenConnection();

            void OpenConnectionAgain()
            {
                unitOfWork.OpenConnection();
            }

            Assert.DoesNotThrow(OpenConnectionAgain);
        }

        [Test]
        public void If_OptionsFactory_returns_null_default_options_are_used_as_fallback()
        {
            var sqliteDatabaseConnectionString = new SQLiteDatabaseConnectionString("Data Source = whatever.db");
            var sqliteDatabaseConnectionFactory = new SQLiteDatabaseConnectionFactory(sqliteDatabaseConnectionString);

            UnitOfWorkOptionsProviderFunc optsFunc = () => null;

            using var unitOfWork = new SynchronousUnitOfWork(sqliteDatabaseConnectionFactory, optsFunc);

            Assert.That(unitOfWork.Options, Is.EqualTo(UnitOfWorkOptions.Default));
        }
    }
}