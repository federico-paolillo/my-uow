using Microsoft.Extensions.DependencyInjection;
using System;

namespace FP.UoW.DependencyInjection
{
    /// <inheritdoc/>
    internal sealed class UnitOfWorkServiceBuilder : IUnitOfWorkServiceBuilder
    {
        public IServiceCollection ServiceCollection { get; private set; }

        /// <inheritdoc/>
        internal UnitOfWorkServiceBuilder(IServiceCollection serviceCollection)
        {
            ServiceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
        }
    }
}
