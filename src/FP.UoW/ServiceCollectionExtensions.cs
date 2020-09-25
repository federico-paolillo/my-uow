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
        public static IUnitOfWorkServiceBuilder AddUoW(this IServiceCollection services)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));

            services.AddScoped<UnitOfWork>();

            services.AddScoped<IDatabaseUnitOfWork>(sp => sp.GetRequiredService<UnitOfWork>());
            services.AddScoped<IDatabaseSession>(sp => sp.GetRequiredService<UnitOfWork>());

            return new UnitOfWorkServiceBuilder(services);
        }
    }
}
