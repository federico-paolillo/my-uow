using Microsoft.Extensions.DependencyInjection;

namespace FP.UoW
{
    public interface IUnitOfWorkServiceBuilder
    {
        public IServiceCollection ServiceCollection { get; }
    }
}
