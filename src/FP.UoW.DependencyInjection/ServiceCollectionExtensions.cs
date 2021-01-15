using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System;

namespace FP.UoW.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds an Unit of Work to the <see cref="IServiceCollection" /> specified
        /// The Unit of Work will use a <see cref="ServiceLifetime.Scoped" /> lifetime
        /// </summary>
        public static UnitOfWorkServiceBuilder AddUoW(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddScoped<UnitOfWork>();

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UnitOfWork>());
            services.AddScoped<IDatabaseSession>(sp => sp.GetRequiredService<UnitOfWork>());

            return new UnitOfWorkServiceBuilder(services);
        }

        /// <summary>
        /// Adds a Synchronous Unit of Work to the <see cref="IServiceCollection"/> specified.
        /// The Synchronous Unit Of Work will use a <see cref="ServiceLifetime.Scoped"/> lifetime.
        /// </summary>
        public static UnitOfWorkServiceBuilder AddSynchronousUoW(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.
        }
    }
}