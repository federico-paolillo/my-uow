using Microsoft.Extensions.DependencyInjection;

namespace FP.UoW.DependencyInjection
{
    /// <summary>
    /// Exposes extension methods to further configure the Unit Of Work library with Dependency Injection
    /// </summary>
    /// <remarks>See specific database drivers for available methods</remarks>
    public interface IUnitOfWorkServiceBuilder
    {
        public IServiceCollection ServiceCollection { get; }
    }
}
