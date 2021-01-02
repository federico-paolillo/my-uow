using Microsoft.Extensions.DependencyInjection;

using System;

namespace FP.UoW.DependencyInjection
{
    public sealed class UnitOfWorkServiceBuilder
    {
        internal UnitOfWorkServiceBuilder(IServiceCollection serviceCollection)
        {
            ServiceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
        }

        public IServiceCollection ServiceCollection { get; }
    }
}