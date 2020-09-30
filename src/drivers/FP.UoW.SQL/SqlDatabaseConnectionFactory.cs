using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace FP.UoW.Sql
{
    /// <summary>
    ///     Creates database connections for MSSQL database
    /// </summary>
    public sealed class SqlDatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private readonly SqlDatabaseConnectionString connectionString;

        public SqlDatabaseConnectionFactory(SqlDatabaseConnectionString connectionString)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public Task<DbConnection> MakeNewAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            DbConnection connection = new SqlConnection(connectionString.Value);

            return Task.FromResult(connection);
        }
    }
}