using System;
using FP.UoW.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace FP.UoW.SQLite.DependencyInjection
{
    public static class UoWServiceBuilderExtensions
    {
        /// <summary>
        ///     Add SQLite specific types for the Unit of Work
        /// </summary>
        public static IServiceCollection ForSQLite(this UnitOfWorkServiceBuilder builder, string connectionString)
        {
            if (builder is null) throw new ArgumentNullException(nameof(builder));

            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException($"'{nameof(connectionString)}' cannot be null or empty",
                    nameof(connectionString));

            var sqlConnectionString = SQLiteDatabaseConnectionString.From(connectionString);

            builder.ServiceCollection.AddSingleton(sqlConnectionString);
            builder.ServiceCollection.AddTransient<IDatabaseConnectionFactory, SQLiteDatabaseConnectionFactory>();

            return builder.ServiceCollection;
        }
    }
}