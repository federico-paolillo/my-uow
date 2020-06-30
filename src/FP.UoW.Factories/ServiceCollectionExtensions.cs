using FP.UoW.Factories;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlUoW(this IServiceCollection services, string connectionString, ServiceLifetime uowLifetime = ServiceLifetime.Scoped)
        {
            return services.AddUoW<SqlConnectionFactory>(connectionString, uowLifetime);
        }

        public static IServiceCollection AddSQLiteUoW(this IServiceCollection services, string connectionString, ServiceLifetime uowLifetime = ServiceLifetime.Scoped)
        {
            return services.AddUoW<SQLiteConnectionFactory>(connectionString, uowLifetime);
        }
    }
}
