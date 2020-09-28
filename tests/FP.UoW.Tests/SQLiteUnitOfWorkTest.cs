using Dapper;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.IO;
using System.Runtime.CompilerServices;
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

        private IUnitOfWork unitOfWork = null;

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

            unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        }

        [Test]
        public async Task Reads_and_writes_from_SQLite()
        {
            await unitOfWork.BeginTransactionAsync()
                .ConfigureAwait(continueOnCapturedContext: false);

            await CreateTableAsync()
                .ConfigureAwait(continueOnCapturedContext: false);

            await InsertRowAsync()
                .ConfigureAwait(continueOnCapturedContext: false);

            await ReadRowAsync()
                .ConfigureAwait(continueOnCapturedContext: false);

            await unitOfWork.CommitTransactionAsync()
                .ConfigureAwait(continueOnCapturedContext: false);
        }

        private async Task CreateTableAsync()
        {
            await unitOfWork.Connection.ExecuteAsync(@"
                CREATE TABLE TestModels(
                    Id TEXT PRIMARY KEY,
                    ColumnOne TEXT NOT NULL,
                    ColumnTwo TEXT NOT NULL
                );", transaction: unitOfWork.Transaction)
            .ConfigureAwait(continueOnCapturedContext: false);
        }

        private async Task InsertRowAsync()
        {
            await unitOfWork.Connection.ExecuteAsync(@"
                    INSERT INTO TestModels(Id, ColumnOne, ColumnTwo) VALUES('Pippo', 'Pluto', 'Garbage');
                ", transaction: unitOfWork.Transaction)
            .ConfigureAwait(continueOnCapturedContext: false);
        }

        private async Task ReadRowAsync()
        {
            var testModel = await unitOfWork.Connection.QuerySingleOrDefaultAsync<TestModel>(@"
                    SELECT * FROM TestModels WHERE Id = 'Pippo';
                ", transaction: unitOfWork.Transaction)
            .ConfigureAwait(continueOnCapturedContext: false);
            
            Assert.That(testModel.Id, Is.EqualTo("Pippo"));
            Assert.That(testModel.ColumnOne, Is.EqualTo("Pluto"));
            Assert.That(testModel.ColumnTwo, Is.EqualTo("Garbage"));
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