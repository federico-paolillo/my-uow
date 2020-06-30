using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace FP.UoW.Factories
{
    /// <summary>
    /// Creates database connections for MSSQL database
    /// </summary>
    internal sealed class SqlConnectionFactory : IDatabaseConnectionFactory
    {
        private readonly DatabaseConnectionString connectionString;

        public SqlConnectionFactory(DatabaseConnectionString connectionString)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public Task<DbConnection> MakeDatabaseConnectionAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            DbConnection connection = new SqlConnection(connectionString.Value);

            return Task.FromResult(connection);
        }
    }
}
