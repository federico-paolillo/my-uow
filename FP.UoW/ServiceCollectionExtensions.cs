using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds an Unit of Work to the <see cref="IServiceCollection"/> specified, optionally using the <see cref="ServiceLifetime"/> provided.
        /// By default the Unit of Work will use a <see cref="ServiceLifetime.Scoped"/> lifetime.
        /// </summary>
        public static IServiceCollection AddUoW(this IServiceCollection services, ServiceLifetime uowLifetime = ServiceLifetime.Scoped)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));

            return services;
        }
    }
}
