using System;
using FP.UoW;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds an Unit of Work to the <see cref="IServiceCollection" /> specified
        /// The Unit of Work will use a <see cref="ServiceLifetime.Scoped" /> lifetime
        /// </summary>
        public static IUnitOfWorkServiceBuilder AddUoW(this IServiceCollection services)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));

            services.AddTransient<IUnitOfWorkFactory, UnitOfWorkFactory>();

            services.AddScoped<UnitOfWork>();

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UnitOfWork>());
            services.AddScoped<IDatabaseSession>(sp => sp.GetRequiredService<UnitOfWork>());

            return new UnitOfWorkServiceBuilder(services);
        }
    }
}