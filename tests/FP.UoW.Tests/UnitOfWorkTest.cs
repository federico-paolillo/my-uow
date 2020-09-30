using System.Threading.Tasks;
using FP.UoW.SQLite;
using NUnit.Framework;

namespace FP.UoW.Tests
{
    public class UnitOfWorkTest
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
                    .ConfigureAwait(continueOnCapturedContext: false);
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
                    .ConfigureAwait(continueOnCapturedContext: false);
            }

            Assert.That(BeginTransactionAgainAsync, Throws.InvalidOperationException);
        }

        [Test]
        public void Unit_of_Work_complains_if_Connection_Factory_Returns_null()
        {
        }
        
        [Test]
        public void Connection_can_t_be_closed_without_commiting_or_rolling_back_the_transaction()
        {
        }
    }
}