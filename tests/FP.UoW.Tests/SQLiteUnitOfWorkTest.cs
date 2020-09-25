using Dapper;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FP.UoW.Tests
{
    public sealed class TestModel
    {
        public string Id { get; set; }

        public string ColumnOne { get; set; }

        public string ColumnTwo { get; set; }
    }

    public class SQLiteUnitOfWorkTest
    {
        private string databaseFileName = null;

        private ServiceProvider serviceProvider = null;

        private IServiceScope serviceScope = null;

        private IDatabaseUnitOfWork unitOfWork = null;

        [SetUp]
        public void Setup()
        {
            databaseFileName = Path.GetRandomFileName();

            var databaseConnectionString = $"Data Source = {databaseFileName}";

            serviceProvider = new ServiceCollection()
                .AddUoW()
                .ForSQLite(databaseConnectionString)
                .BuildServiceProvider();

            serviceScope = serviceProvider.CreateScope();

            unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IDatabaseUnitOfWork>();
        }

        [Test]
        public async Task Creates_a_table_and_inserts_a_row_then_reads_the_row_changes_it_and_rolls_back_the_changes()
        {
            //Creates a table and inserts a row...

            await unitOfWork.OpenConnectionAsync()
                .ConfigureAwait(continueOnCapturedContext: false);

            await unitOfWork.BeginTransactionAsync()
                .ConfigureAwait(continueOnCapturedContext: false);

            await unitOfWork.Connection.ExecuteAsync(@"
                CREATE TABLE TestModels(
                    Id TEXT PRIMARY KEY,
                    ColumnOne TEXT NOT NULL,
                    ColumnTwo TEXT NOT NULL
                );", transaction: unitOfWork.Transaction)
                .ConfigureAwait(continueOnCapturedContext: false);

            var testModel = new TestModel { Id = Guid.NewGuid().ToString(), ColumnOne = "Value #1", ColumnTwo = "Value #2" };

            await unitOfWork.Connection.ExecuteAsync(@"
                INSERT INTO TestModels(Id, ColumnOne, ColumnTwo)
                VALUES (
                    @Id, 
                    @ColumnOne, 
                    @ColumnTwo
                );", param: testModel, transaction: unitOfWork.Transaction)
                .ConfigureAwait(continueOnCapturedContext: false);

            await unitOfWork.CommitTransactionAsync()
                .ConfigureAwait(continueOnCapturedContext: false);

            //...then reads the row...

            await unitOfWork.BeginTransactionAsync()
                .ConfigureAwait(continueOnCapturedContext: false);

            var testModelFromDatabase = await unitOfWork.Connection.QueryFirstOrDefaultAsync<TestModel>(@"
                SELECT * FROM TestModels
                ;", param: testModel, transaction: unitOfWork.Transaction)
                 .ConfigureAwait(continueOnCapturedContext: false);

            Assert.That(testModelFromDatabase.Id, Is.EqualTo(testModel.Id));

            //...makes a change and rollbacks it

            await unitOfWork.Connection.ExecuteAsync(@"
                UPDATE TestModels
                SET ColumnOne = 'Something else'
                WHERE Id = @Id
                ;", param: new { testModel.Id }, transaction: unitOfWork.Transaction)
            .ConfigureAwait(continueOnCapturedContext: false);

            await unitOfWork.RollbackTransactionAsync()
                .ConfigureAwait(continueOnCapturedContext: false);

            await unitOfWork.CloseConnectionAsync()
                .ConfigureAwait(continueOnCapturedContext: false);
        }

        [TearDown]
        public void TearDown()
        {
            serviceScope?.Dispose();
            serviceProvider?.Dispose();

            File.Delete(databaseFileName);
        }
    }
}