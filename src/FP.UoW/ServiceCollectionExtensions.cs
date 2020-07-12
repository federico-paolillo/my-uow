using FP.UoW;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds an Unit of Work to the <see cref="IServiceCollection"/> specified.
        /// The Unit of Work will use a <see cref="ServiceLifetime.Scoped"/> lifetime (one per request).
        /// </summary>
        public static IServiceCollection AddUoW<TDatabaseConnectionFactory>(this IServiceCollection services, string connectionString)
            where TDatabaseConnectionFactory : class, IDatabaseConnectionFactory
        {
            if (services is null) throw new ArgumentNullException(nameof(services));

            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("No connection string was specified", nameof(connectionString));

            var databaseConnectionString = DatabaseConnectionString.From(connectionString);

            services.AddSingleton(databaseConnectionString);

            services.AddTransient<IDatabaseConnectionFactory, TDatabaseConnectionFactory>();

            services.AddScoped<UnitOfWork>();

            services.AddScoped<IDatabaseUnitOfWork>(sp => sp.GetRequiredService<UnitOfWork>());
            services.AddScoped<IDatabaseSession>(sp => sp.GetRequiredService<UnitOfWork>());

            return services;
        }
    }
}
