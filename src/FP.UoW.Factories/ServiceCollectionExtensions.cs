using FP.UoW.Factories;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add Microsoft SQL Server specific types for the Unit of Work
        /// </summary>
        public static IServiceCollection AddSqlUoW(this IServiceCollection services, string connectionString)
        {
            return services.AddUoW<SqlConnectionFactory>(connectionString);
        }

        /// <summary>
        /// Add Microsoft SQLite specific types for the Unit of Work
        /// </summary>
        public static IServiceCollection AddSQLiteUoW(this IServiceCollection services, string connectionString)
        {
            return services.AddUoW<SQLiteConnectionFactory>(connectionString);
        }
    }
}
