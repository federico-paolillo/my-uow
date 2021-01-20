using Microsoft.Extensions.DependencyInjection;

using System;

namespace FP.UoW.DependencyInjection
{
    /// <summary>
    /// Information to configure the Unit of Work with the Microsoft.Extensions.DependencyInjection container.
    /// </summary>
    public sealed class UnitOfWorkServiceBuilder
    {
        internal UnitOfWorkServiceBuilder(IServiceCollection serviceCollection)
        {
            ServiceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
        }

        public IServiceCollection ServiceCollection { get; }
    }
}