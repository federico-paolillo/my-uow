using Dapper;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FP.UoW.Tests
{
    public class SQLiteUnitOfWorkTest
    {
        private string databaseFileName = null;
        private ServiceProvider serviceProvider = null;
        private IDatabaseUnitOfWork unitOfWork = null;
        private IDatabaseSession databaseSession = null;

        [SetUp]
        public void Setup()
        {
            databaseFileName = Path.GetRandomFileName();

            var databaseConnectionString = $"Data Source = {databaseFileName}";

            serviceProvider = new ServiceCollection()
                .AddSQLiteUoW(databaseConnectionString)
                .BuildServiceProvider();

            unitOfWork = serviceProvider.GetRequiredService<IDatabaseUnitOfWork>();
            databaseSession = serviceProvider.GetRequiredService<IDatabaseSession>();
        }

        [Test]
        public async Task Creates_a_table_and_inserts_a_row_then_reads_the_row_changes_it_and_rolls_back_the_changes()
        {
            //Creates a table and inserts a row...

            await unitOfWork.BeginAsync()
                .ConfigureAwait(continueOnCapturedContext: false);

            await databaseSession.Connection.ExecuteAsync(@"
                CREATE TABLE TestModels(
                    Id TEXT PRIMARY KEY,
                    ColumnOne TEXT NOT NULL,
                    ColumnTwo TEXT NOT NULL
                );", transaction: databaseSession.Transaction)
                .ConfigureAwait(continueOnCapturedContext: false);

            var testModel = new TestModel { Id = Guid.NewGuid().ToString(), ColumnOne = "Value #1", ColumnTwo = "Value #2" };

            await databaseSession.Connection.ExecuteAsync(@"
                INSERT INTO TestModels(Id, ColumnOne, ColumnTwo)
                VALUES (
                    @Id, 
                    @ColumnOne, 
                    @ColumnTwo
                );", param: testModel, transaction: databaseSession.Transaction)
                .ConfigureAwait(continueOnCapturedContext: false);

            await unitOfWork.CommitAsync()
                .ConfigureAwait(continueOnCapturedContext: false);

            //..then reads the row, changes it and rolls back the changes

            await unitOfWork.BeginAsync()
                .ConfigureAwait(continueOnCapturedContext: false);

            var testModelFromDatabase = await databaseSession.Connection.QueryFirstOrDefaultAsync<TestModel>(@"
                SELECT * FROM TestModels
                ;", param: testModel, transaction: databaseSession.Transaction)
                 .ConfigureAwait(continueOnCapturedContext: false);

            Assert.That(testModelFromDatabase.Id, Is.EqualTo(testModel.Id));

            await databaseSession.Connection.ExecuteAsync(@"
                UPDATE TestModels
                SET ColumnOne = 'Something else'
                WHERE Id = @Id
                ;", param: new { Id = testModel.Id }, transaction: databaseSession.Transaction)
            .ConfigureAwait(continueOnCapturedContext: false);

            await unitOfWork.RollbackAsync()
                .ConfigureAwait(continueOnCapturedContext: false);
        }

        [TearDown]
        public void TearDown()
        {
            serviceProvider?.Dispose();

            File.Delete(databaseFileName);
        }
    }
}