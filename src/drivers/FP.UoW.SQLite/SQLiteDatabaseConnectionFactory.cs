using System;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;

namespace FP.UoW.SQLite
{
    /// <summary>
    ///     Creates database connections for a SQLite database
    /// </summary>
    public sealed class SQLiteDatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private readonly SQLiteDatabaseConnectionString connectionString;

        public SQLiteDatabaseConnectionFactory(SQLiteDatabaseConnectionString connectionString)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public Task<DbConnection> MakeNewAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            DbConnection connection = new SQLiteConnection(connectionString.Value);

            return Task.FromResult(connection);
        }
    }
}