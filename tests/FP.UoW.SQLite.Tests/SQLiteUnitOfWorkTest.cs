using System.IO;
using System.Threading.Tasks;
using Dapper;
using FP.UoW.DependencyInjection;
using FP.UoW.SQLite.DependencyInjection;
using FP.UoW.SQLite.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FP.UoW.SQLite.Tests
{
    public sealed class SQLiteUnitOfWorkTest
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
        public async Task Reads_and_writes_from_SQLite()
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
        public async Task Writes_to_SQLite_can_be_rolled_back()
        {
            await unitOfWork.BeginTransactionAsync()
                .ConfigureAwait(false);

            await CreateTableAsync()
                .ConfigureAwait(false);

            await unitOfWork.CommitTransactionAsync()
                .ConfigureAwait(false);

            await unitOfWork.BeginTransactionAsync()
                .ConfigureAwait(false);

            await InsertRandomTestModelRowAsync()
                .ConfigureAwait(false);

            await InsertRandomTestModelRowAsync()
                .ConfigureAwait(false);

            await unitOfWork.RollbackTransactionAsync()
                .ConfigureAwait(false);

            await unitOfWork.OpenConnectionAsync()
                .ConfigureAwait(false);

            await AssertNoTestModelRowsAsync()
                .ConfigureAwait(false);

            await unitOfWork.CloseConnectionAsync()
                .ConfigureAwait(false);
        }

        [TearDown]
        public void TearDown()
        {
            serviceScope?.Dispose();
            serviceProvider?.Dispose();

            File.Delete(databaseFileName);
        }

        private async Task AssertNoTestModelRowsAsync()
        {
            var count = await unitOfWork.Connection.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM TestModels;
            ", transaction: unitOfWork.Transaction);

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
                    Id TEXT PRIMARY KEY,
                    ColumnOne TEXT NOT NULL,
                    ColumnTwo TEXT NOT NULL
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
    }
}