using FP.UoW;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds an Unit of Work to the <see cref="IServiceCollection"/> specified, optionally using the <see cref="ServiceLifetime"/> provided.
        /// By default the Unit of Work will use a <see cref="ServiceLifetime.Scoped"/> lifetime.
        /// </summary>
        public static IServiceCollection AddUoW<TDatabaseConnectionFactory>(this IServiceCollection services, string connectionString, ServiceLifetime uowLifetime = ServiceLifetime.Scoped)
            where TDatabaseConnectionFactory : class, IDatabaseConnectionFactory
        {
            if (services is null) throw new ArgumentNullException(nameof(services));

            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("No connection string was specified", nameof(connectionString));

            var databaseConnectionString = DatabaseConnectionString.From(connectionString);

            services.AddSingleton(databaseConnectionString);
            services.AddSingleton<IDatabaseConnectionFactory, TDatabaseConnectionFactory>();

            static object ResolveUnitOfWork(IServiceProvider serviceProvider)
            {
                return serviceProvider.GetRequiredService<UnitOfWork>();
            }

            var unitOfWorkServiceDescriptor = new ServiceDescriptor(typeof(UnitOfWork), typeof(UnitOfWork), uowLifetime);

            //ServiceLifetime is "meaningless" here because we always forward resolution to Unit of Work which has its own lifetime
            //We can safely use Transient here because the actual lifetime will be the one specified by the parameter uowLifetime

            var databaseUnitOfWorkServiceDescriptor = new ServiceDescriptor(typeof(IDatabaseUnitOfWork), ResolveUnitOfWork, ServiceLifetime.Transient);
            var databaseSessionServiceDescriptor = new ServiceDescriptor(typeof(IDatabaseSession), ResolveUnitOfWork, ServiceLifetime.Transient);

            services.Add(unitOfWorkServiceDescriptor);
            services.Add(databaseUnitOfWorkServiceDescriptor);
            services.Add(databaseSessionServiceDescriptor);

            return services;
        }
    }
}
