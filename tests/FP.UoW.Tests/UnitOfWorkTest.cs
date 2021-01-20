using FP.UoW.SQLite;

using Moq;

using NUnit.Framework;

using System.Threading;
using System.Threading.Tasks;

namespace FP.UoW.Tests
{
    public sealed class UnitOfWorkTest
    {
        [Test]
        public async Task A_Connection_can_t_be_open_twice()
        {
            var sqliteDatabaseConnectionString = new SQLiteDatabaseConnectionString("Data Source = whatever.db");
            var sqliteDatabaseConnectionFactory = new SQLiteDatabaseConnectionFactory(sqliteDatabaseConnectionString);

            using var unitOfWork = new UnitOfWork(sqliteDatabaseConnectionFactory);

            await unitOfWork.OpenConnectionAsync()
                .ConfigureAwait(false);

            async Task OpenConnectionAgainAsync()
            {
                await unitOfWork.OpenConnectionAsync()
                    .ConfigureAwait(false);
            }

            Assert.That(OpenConnectionAgainAsync, Throws.InvalidOperationException);
        }

        [Test]
        public async Task A_Transaction_can_t_begin_twice()
        {
            var sqliteDatabaseConnectionString = new SQLiteDatabaseConnectionString("Data Source = whatever.db");
            var sqliteDatabaseConnectionFactory = new SQLiteDatabaseConnectionFactory(sqliteDatabaseConnectionString);

            using var unitOfWork = new UnitOfWork(sqliteDatabaseConnectionFactory);

            await unitOfWork.BeginTransactionAsync()
                .ConfigureAwait(false);

            async Task BeginTransactionAgainAsync()
            {
                await unitOfWork.BeginTransactionAsync()
                    .ConfigureAwait(false);
            }

            Assert.That(BeginTransactionAgainAsync, Throws.InvalidOperationException);
        }

        [Test]
        public void Unit_of_Work_complains_if_Connection_Factory_Returns_null()
        {
            var brokenConnectionFactoryMock = new Mock<IDatabaseConnectionFactory>();

            brokenConnectionFactoryMock.Setup(m => m.MakeNewAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(value: null);

            using var unitOfWork = new UnitOfWork(brokenConnectionFactoryMock.Object);

            async Task TryOpenConnectionAsync()
            {
                await unitOfWork.OpenConnectionAsync()
                    .ConfigureAwait(false);
            }

            Assert.That(TryOpenConnectionAsync, Throws.InvalidOperationException);
        }

        [Test]
        public async Task Connection_can_t_be_closed_without_commiting_or_rolling_back_the_transaction()
        {
            var sqliteDatabaseConnectionString = new SQLiteDatabaseConnectionString("Data Source = whatever.db");
            var sqliteDatabaseConnectionFactory = new SQLiteDatabaseConnectionFactory(sqliteDatabaseConnectionString);

            using var unitOfWork = new UnitOfWork(sqliteDatabaseConnectionFactory);

            await unitOfWork.BeginTransactionAsync()
                .ConfigureAwait(false);

            async Task TryCloseConnectionAsync()
            {
                await unitOfWork.CloseConnectionAsync()
                    .ConfigureAwait(false);
            }

            Assert.That(TryCloseConnectionAsync, Throws.InvalidOperationException);
        }

        [Test]
        public void Transaction_can_t_be_rolled_back_without_beginning_it()
        {
            var sqliteDatabaseConnectionString = new SQLiteDatabaseConnectionString("Data Source = whatever.db");
            var sqliteDatabaseConnectionFactory = new SQLiteDatabaseConnectionFactory(sqliteDatabaseConnectionString);

            using var unitOfWork = new UnitOfWork(sqliteDatabaseConnectionFactory);

            async Task TryRollbackTransactionAsync()
            {
                await unitOfWork.RollbackTransactionAsync()
                    .ConfigureAwait(false);
            }

            Assert.That(TryRollbackTransactionAsync, Throws.InvalidOperationException);
        }

        [Test]
        public void Transaction_can_t_be_commited_without_beginning_it()
        {
            var sqliteDatabaseConnectionString = new SQLiteDatabaseConnectionString("Data Source = whatever.db");
            var sqliteDatabaseConnectionFactory = new SQLiteDatabaseConnectionFactory(sqliteDatabaseConnectionString);

            using var unitOfWork = new UnitOfWork(sqliteDatabaseConnectionFactory);

            async Task TryCommitTransactionAsync()
            {
                await unitOfWork.CommitTransactionAsync()
                    .ConfigureAwait(false);
            }

            Assert.That(TryCommitTransactionAsync, Throws.InvalidOperationException);
        }

        [Test]
        public void Closing_Connection_that_are_not_open_does_nothing()
        {
            var sqliteDatabaseConnectionString = new SQLiteDatabaseConnectionString("Data Source = whatever.db");
            var sqliteDatabaseConnectionFactory = new SQLiteDatabaseConnectionFactory(sqliteDatabaseConnectionString);

            using var unitOfWork = new UnitOfWork(sqliteDatabaseConnectionFactory);

            async Task TryCloseConnectionAsync()
            {
                await unitOfWork.CloseConnectionAsync()
                    .ConfigureAwait(false);
            }

            Assert.That(TryCloseConnectionAsync, Throws.Nothing);
        }

        [Test]
        public async Task AsyncDispose_actually_cleans_up()
        {
            var sqliteDatabaseConnectionString = new SQLiteDatabaseConnectionString("Data Source = whatever.db");
            var sqliteDatabaseConnectionFactory = new SQLiteDatabaseConnectionFactory(sqliteDatabaseConnectionString);

            //Note: never use an async disposable like this. It might make a mess if the .ctor fails. For the test it's ok

            var unitOfWork = new UnitOfWork(sqliteDatabaseConnectionFactory);

            await using (unitOfWork)
            {
                await unitOfWork.OpenConnectionAsync()
                    .ConfigureAwait(continueOnCapturedContext: false);

                await unitOfWork.BeginTransactionAsync()
                    .ConfigureAwait(continueOnCapturedContext: false);

                //Pretending to forget to close/rollback

                Assert.That(unitOfWork.Connection, Is.Not.Null);
                Assert.That(unitOfWork.Transaction, Is.Not.Null);
            }

            Assert.That(unitOfWork.Connection, Is.Null);
            Assert.That(unitOfWork.Transaction, Is.Null);
        }

        [Test]
        public async Task Multiple_transaction_errors_are_ignored_when_the_appropriate_option_is_specified()
        {
            var sqliteDatabaseConnectionString = new SQLiteDatabaseConnectionString("Data Source = whatever.db");
            var sqliteDatabaseConnectionFactory = new SQLiteDatabaseConnectionFactory(sqliteDatabaseConnectionString);

            var options = new UnitOfWorkOptions
            {
                ThrowOnMultipleTransactionsAttempts = false,
                ThrowOnMultipleConnectionsAttempts = true
            };

            await using var unitOfWork = new UnitOfWork(sqliteDatabaseConnectionFactory, options);

            await unitOfWork.BeginTransactionAsync()
                .ConfigureAwait(continueOnCapturedContext: false);

            async Task BeginTransactionAgainAsync()
            {
                await unitOfWork.BeginTransactionAsync()
                    .ConfigureAwait(continueOnCapturedContext: false);
            }

            Assert.DoesNotThrowAsync(BeginTransactionAgainAsync);
        }

        [Test]
        public async Task Multiple_connections_errors_are_ignored_when_the_appropriate_option_is_specified()
        {
            var sqliteDatabaseConnectionString = new SQLiteDatabaseConnectionString("Data Source = whatever.db");
            var sqliteDatabaseConnectionFactory = new SQLiteDatabaseConnectionFactory(sqliteDatabaseConnectionString);

            var options = new UnitOfWorkOptions
            {
                ThrowOnMultipleTransactionsAttempts = true,
                ThrowOnMultipleConnectionsAttempts = false
            };

            await using var unitOfWork = new UnitOfWork(sqliteDatabaseConnectionFactory, options);

            await unitOfWork.OpenConnectionAsync()
                .ConfigureAwait(continueOnCapturedContext: false);

            async Task OpenConnectionAgainAsync()
            {
                await unitOfWork.OpenConnectionAsync()
                    .ConfigureAwait(continueOnCapturedContext: false);
            }

            Assert.DoesNotThrowAsync(OpenConnectionAgainAsync);
        }

        [Test]
        public async Task If_OptionsFactory_returns_null_default_options_are_used_as_fallback()
        {
            var sqliteDatabaseConnectionString = new SQLiteDatabaseConnectionString("Data Source = whatever.db");
            var sqliteDatabaseConnectionFactory = new SQLiteDatabaseConnectionFactory(sqliteDatabaseConnectionString);

            UnitOfWorkOptionsProviderFunc optsFunc = () => null;

            await using var unitOfWork = new UnitOfWork(sqliteDatabaseConnectionFactory, optsFunc);

            Assert.That(unitOfWork.Options, Is.EqualTo(UnitOfWorkOptions.Default));
        }
    }
}