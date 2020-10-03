using System.IO;
using Dapper;
using FP.UoW.DependencyInjection;
using FP.UoW.SQLite.DependencyInjection;
using FP.UoW.SQLite.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FP.UoW.SQLite.Tests
{
    public sealed class SQLiteUnitOfWorkSyncTest
    {
        private string databaseFileName;

        private TestModel randomModel;

        private ServiceProvider serviceProvider;

        private IServiceScope serviceScope;

        private IUnitOfWork unitOfWork;

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

            randomModel = TestModel.Random();
        }

        [Test]
        public void Reads_and_writes_from_SQLite()
        {
            unitOfWork.BeginTransaction();

            CreateTable();

            InsertTestModelRow();

            AssertCanReadTestModelRow();

            unitOfWork.CommitTransaction();
        }

        [Test]
        public void Writes_to_SQLite_can_be_rolled_back()
        {
            unitOfWork.BeginTransaction();

            CreateTable();

            unitOfWork.CommitTransaction();

            unitOfWork.BeginTransaction();

            InsertRandomTestModelRow();

            InsertRandomTestModelRow();

            unitOfWork.RollbackTransaction();

            unitOfWork.OpenConnection();

            AssertNoTestModelRows();

            unitOfWork.CloseConnection();
        }

        [TearDown]
        public void TearDown()
        {
            serviceScope?.Dispose();
            serviceProvider?.Dispose();

            File.Delete(databaseFileName);
        }

        private void AssertNoTestModelRows()
        {
            var count = unitOfWork.Connection.ExecuteScalar<int>(@"
                SELECT COUNT(*) FROM TestModels;
            ", transaction: unitOfWork.Transaction);

            Assert.That(count, Is.Zero);
        }

        private void InsertRandomTestModelRow()
        {
            var someRandomTestModel = TestModel.Random();

            unitOfWork.Connection.Execute(@"
                    INSERT INTO TestModels(Id, ColumnOne, ColumnTwo) VALUES(@Id, @ColumnOne, @ColumnTwo);
                ", transaction: unitOfWork.Transaction, param: someRandomTestModel);
        }

        private void CreateTable()
        {
            unitOfWork.Connection.Execute(@"
                CREATE TABLE TestModels(
                    Id TEXT PRIMARY KEY,
                    ColumnOne TEXT NOT NULL,
                    ColumnTwo TEXT NOT NULL
                );", transaction: unitOfWork.Transaction);
        }

        private void AssertCanReadTestModelRow()
        {
            var testModel = unitOfWork.Connection.QuerySingleOrDefault<TestModel>(@"
                    SELECT * FROM TestModels WHERE Id = @Id;
                ", transaction: unitOfWork.Transaction, param: randomModel);

            Assert.That(testModel.Id, Is.EqualTo(randomModel.Id));
            Assert.That(testModel.ColumnOne, Is.EqualTo(randomModel.ColumnOne));
            Assert.That(testModel.ColumnTwo, Is.EqualTo(randomModel.ColumnTwo));
        }

        private void InsertTestModelRow()
        {
            unitOfWork.Connection.Execute(@"
                    INSERT INTO TestModels(Id, ColumnOne, ColumnTwo) VALUES(@Id, @ColumnOne, @ColumnTwo);
                ", transaction: unitOfWork.Transaction, param: randomModel);
        }
    }
}