using System.Threading;
using System.Threading.Tasks;
using FP.UoW.SQLite;
using Moq;
using NUnit.Framework;

namespace FP.UoW.Tests
{
    public sealed class UnitOfWorkAsyncTest
    {
        [Test]
        public async Task A_Connection_can_t_be_open_twice()
        {
            var sqliteDatabaseConnectionString = SQLiteDatabaseConnectionString.From("Data Source = whatever.db");
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
            var sqliteDatabaseConnectionString = SQLiteDatabaseConnectionString.From("Data Source = whatever.db");
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
            var sqliteDatabaseConnectionString = SQLiteDatabaseConnectionString.From("Data Source = whatever.db");
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
            var sqliteDatabaseConnectionString = SQLiteDatabaseConnectionString.From("Data Source = whatever.db");
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
            var sqliteDatabaseConnectionString = SQLiteDatabaseConnectionString.From("Data Source = whatever.db");
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
            var sqliteDatabaseConnectionString = SQLiteDatabaseConnectionString.From("Data Source = whatever.db");
            var sqliteDatabaseConnectionFactory = new SQLiteDatabaseConnectionFactory(sqliteDatabaseConnectionString);

            using var unitOfWork = new UnitOfWork(sqliteDatabaseConnectionFactory);

            async Task TryCloseConnectionAsync()
            {
                await unitOfWork.CloseConnectionAsync()
                    .ConfigureAwait(false);
            }

            Assert.That(TryCloseConnectionAsync, Throws.Nothing);
        }
    }
}