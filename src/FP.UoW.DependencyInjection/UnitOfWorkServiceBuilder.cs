using Microsoft.Extensions.DependencyInjection;
using System;

namespace FP.UoW.DependencyInjection
{
    public sealed class UnitOfWorkServiceBuilder
    {
        public IServiceCollection ServiceCollection { get; }

        internal UnitOfWorkServiceBuilder(IServiceCollection serviceCollection)
        {
            ServiceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
        }
    }
}