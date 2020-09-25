using Microsoft.Extensions.DependencyInjection;
using System;

namespace FP.UoW
{
    internal sealed class UnitOfWorkServiceBuilder : IUnitOfWorkServiceBuilder
    {
        public IServiceCollection ServiceCollection { get; private set; }

        internal UnitOfWorkServiceBuilder(IServiceCollection serviceCollection)
        {
            ServiceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
        }
    }
}
