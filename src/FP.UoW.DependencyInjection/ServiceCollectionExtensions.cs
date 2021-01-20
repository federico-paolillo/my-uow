using FP.UoW.Synchronous;

using Microsoft.Extensions.DependencyInjection;

using System;

namespace FP.UoW.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds an Unit of Work to the <see cref="IServiceCollection" /> specified.
        /// The Unit of Work will use a <see cref="ServiceLifetime.Scoped" /> lifetime.
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
        /// Adds the Synchronous variant of the Unit of Work.
        /// The Synchronous Unit Of Work will use a <see cref="ServiceLifetime.Scoped"/> lifetime.
        /// </summary>
        public static UnitOfWorkServiceBuilder AddSynchronousImplementation(this UnitOfWorkServiceBuilder uowBuilder)
        {
            if (uowBuilder is null)
            {
                throw new ArgumentNullException(nameof(uowBuilder));
            }

            uowBuilder.ServiceCollection.AddScoped<SynchronousUnitOfWork>();

            uowBuilder.ServiceCollection.AddScoped<ISynchronousUnitOfWork>(sp => sp.GetRequiredService<SynchronousUnitOfWork>());
            uowBuilder.ServiceCollection.AddScoped<ISynchronousDatabaseSession>(sp => sp.GetRequiredService<SynchronousUnitOfWork>());

            return uowBuilder;
        }
    }
}