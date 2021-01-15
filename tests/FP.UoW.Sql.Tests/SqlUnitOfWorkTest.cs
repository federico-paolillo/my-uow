using Dapper;

using FP.UoW.DependencyInjection;
using FP.UoW.Sql.DependencyInjection;
using FP.UoW.Sql.Tests.Infrastructure;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using System.Threading.Tasks;

namespace FP.UoW.Sql.Tests
{
    public sealed class SqlUnitOfWorkTest
    {
        private string databaseName;

        private TestModel randomModel;

        private ServiceProvider serviceProvider;

        private IServiceScope serviceScope;

        private IUnitOfWork unitOfWork;

        [SetUp]
        public async Task Setup()
        {
            databaseName = Randomness.DatabaseName();

            await CreateDatabaseAsync()
                .ConfigureAwait(false);

            var databaseConnectionString =
                $@"Data Source = (localdb)\MSSQLLocalDB; Integrated Security = true; Initial Catalog = {databaseName}";

            serviceProvider = new ServiceCollection()
                .AddUoW()
                .ForSql(databaseConnectionString)
                .BuildServiceProvider();

            serviceScope = serviceProvider.CreateScope();

            unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            randomModel = TestModel.Random();
        }

        [TearDown]
        public async Task TearDown()
        {
            serviceScope?.Dispose();
            serviceProvider?.Dispose();

            await DropDatabaseAsync()
                .ConfigureAwait(false);
        }

        [Test]
        public async Task Reads_and_writes_from_Sql()
        {
            await unitOfWork.BeginTransactionAsync()
                .ConfigureAwait(false);

            await CreateTableAsync()
                .ConfigureAwait(false);

            await InsertTestModelRowAsync()
                .ConfigureAwait(false);

            await AssertCanReadTestModelRowAsync()
                .ConfigureAwait(false);

            await unitOfWork.CommitTransactionAsync()
                .ConfigureAwait(false);
        }

        [Test]
        public async Task Writes_to_Sql_can_be_rolled_back()
        {
            //Step 1

            await unitOfWork.BeginTransactionAsync()
                .ConfigureAwait(false);

            await CreateTableAsync()
                .ConfigureAwait(false);

            await unitOfWork.CommitTransactionAsync()
                .ConfigureAwait(false);

            //Step 2

            await unitOfWork.BeginTransactionAsync()
                .ConfigureAwait(false);

            await InsertRandomTestModelRowAsync()
                .ConfigureAwait(false);

            await InsertRandomTestModelRowAsync()
                .ConfigureAwait(false);

            await unitOfWork.RollbackTransactionAsync()
                .ConfigureAwait(false);

            //Step 3

            await unitOfWork.OpenConnectionAsync()
                .ConfigureAwait(false);

            await AssertNoTestModelRowsAsync()
                .ConfigureAwait(false);

            await unitOfWork.CloseConnectionAsync()
                .ConfigureAwait(false);
        }

        private async Task AssertNoTestModelRowsAsync()
        {
            var count = await unitOfWork.Connection.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM TestModels;
            ", transaction: unitOfWork.Transaction)
                .ConfigureAwait(false);

            Assert.That(count, Is.Zero);
        }

        private async Task InsertRandomTestModelRowAsync()
        {
            var someRandomTestModel = TestModel.Random();

            await unitOfWork.Connection.ExecuteAsync(@"
                    INSERT INTO TestModels(Id, ColumnOne, ColumnTwo) VALUES(@Id, @ColumnOne, @ColumnTwo);
                ", transaction: unitOfWork.Transaction, param: someRandomTestModel)
                .ConfigureAwait(false);
        }

        private async Task CreateTableAsync()
        {
            await unitOfWork.Connection.ExecuteAsync(@"
                CREATE TABLE TestModels(
                    Id INT PRIMARY KEY,
                    ColumnOne NVARCHAR(MAX) NOT NULL,
                    ColumnTwo NVARCHAR(MAX) NOT NULL
                );", transaction: unitOfWork.Transaction)
                .ConfigureAwait(false);
        }

        private async Task InsertTestModelRowAsync()
        {
            await unitOfWork.Connection.ExecuteAsync(@"
                    INSERT INTO TestModels(Id, ColumnOne, ColumnTwo) VALUES(@Id, @ColumnOne, @ColumnTwo);
                ", transaction: unitOfWork.Transaction, param: randomModel)
                .ConfigureAwait(false);
        }

        private async Task AssertCanReadTestModelRowAsync()
        {
            var testModel = await unitOfWork.Connection.QuerySingleOrDefaultAsync<TestModel>(@"
                    SELECT * FROM TestModels WHERE Id = @Id;
                ", transaction: unitOfWork.Transaction, param: randomModel)
                .ConfigureAwait(false);

            Assert.That(testModel.Id, Is.EqualTo(randomModel.Id));
            Assert.That(testModel.ColumnOne, Is.EqualTo(randomModel.ColumnOne));
            Assert.That(testModel.ColumnTwo, Is.EqualTo(randomModel.ColumnTwo));
        }

        private async Task DropDatabaseAsync()
        {
            await using var connection =
                new SqlConnection(@"Data Source = (localdb)\MSSQLLocalDB; Integrated Security = true;");

            //Drop any pending Connections

            await connection.ExecuteAsync($@"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;")
                .ConfigureAwait(false);

            await connection.ExecuteAsync($@"DROP DATABASE [{databaseName}]; ")
                .ConfigureAwait(false);
        }

        private async Task CreateDatabaseAsync()
        {
            await using var connection =
                new SqlConnection(@"Data Source = (localdb)\MSSQLLocalDB; Integrated Security = true;");

            await connection.ExecuteAsync($@"CREATE DATABASE [{databaseName}];")
                .ConfigureAwait(false);
        }
    }
}