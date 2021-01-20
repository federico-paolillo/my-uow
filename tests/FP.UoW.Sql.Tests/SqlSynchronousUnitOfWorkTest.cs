using Dapper;

using FP.UoW.DependencyInjection;
using FP.UoW.Sql.DependencyInjection;
using FP.UoW.Sql.Tests.Infrastructure;
using FP.UoW.Synchronous;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FP.UoW.Sql.Tests
{
    public sealed class SqlSynchronousUnitOfWorkTest
    {
        private string databaseName;

        private TestModel randomModel;

        private ServiceProvider serviceProvider;

        private IServiceScope serviceScope;

        private ISynchronousUnitOfWork unitOfWork;

        [SetUp]
        public void Setup()
        {
            databaseName = Randomness.DatabaseName();

            CreateDatabase();

            var databaseConnectionString =
                $@"Data Source = (localdb)\MSSQLLocalDB; Integrated Security = true; Initial Catalog = {databaseName}";

            serviceProvider = new ServiceCollection()
                .AddUoW()
                .AddSynchronousImplementation()
                .ForSql(databaseConnectionString)
                .BuildServiceProvider();

            serviceScope = serviceProvider.CreateScope();

            unitOfWork = serviceScope.ServiceProvider.GetRequiredService<ISynchronousUnitOfWork>();

            randomModel = TestModel.Random();
        }

        [TearDown]
        public void TearDown()
        {
            unitOfWork?.Dispose();
            serviceScope?.Dispose();
            serviceProvider?.Dispose();

            DropDatabase();
        }

        [Test]
        public void Reads_and_writes_from_Sql()
        {
            unitOfWork.BeginTransaction();

            CreateTable();

            InsertTestModelRow();

            AssertCanReadTestModelRow();

            unitOfWork.CommitTransaction();
        }

        [Test]
        public void Writes_to_Sql_can_be_rolled_back()
        {
            //Step 1

            unitOfWork.BeginTransaction();

            CreateTable();

            unitOfWork.CommitTransaction();

            //Step 2

            unitOfWork.BeginTransaction();

            InsertRandomTestModelRow();

            InsertRandomTestModelRow();

            unitOfWork.RollbackTransaction();

            //Step 3

            unitOfWork.OpenConnection();

            AssertNoTestModelRows();

            unitOfWork.CloseConnection();
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
                    Id INT PRIMARY KEY,
                    ColumnOne NVARCHAR(MAX) NOT NULL,
                    ColumnTwo NVARCHAR(MAX) NOT NULL
                );", transaction: unitOfWork.Transaction);
        }

        private void InsertTestModelRow()
        {
            unitOfWork.Connection.Execute(@"
                    INSERT INTO TestModels(Id, ColumnOne, ColumnTwo) VALUES(@Id, @ColumnOne, @ColumnTwo);
                ", transaction: unitOfWork.Transaction, param: randomModel);
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

        private void DropDatabase()
        {
            using var connection =
                new SqlConnection(@"Data Source = (localdb)\MSSQLLocalDB; Integrated Security = true;");

            //Drop any pending Connections

            connection.Execute($@"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;");
            connection.Execute($@"DROP DATABASE [{databaseName}];");
        }

        private void CreateDatabase()
        {
            using var connection =
                new SqlConnection(@"Data Source = (localdb)\MSSQLLocalDB; Integrated Security = true;");

            connection.Execute($@"CREATE DATABASE [{databaseName}];");
        }
    }
}