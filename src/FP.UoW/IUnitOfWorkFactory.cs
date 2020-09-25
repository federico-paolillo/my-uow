using System.Threading;
using System.Threading.Tasks;

namespace FP.UoW
{
    /// <summary>
    /// Builds new instances of <see cref="IDatabaseUnitOfWork"/> to consume
    /// </summary>
    /// <remarks>This interface is required if you want to manually manage the <see cref="IDatabaseUnitOfWork"/> lifetime.</remarks>
    public interface IUnitOfWorkFactory
    {
        Task<IDatabaseUnitOfWork> MakeNewAsync(CancellationToken cancellationToken = default);
    }
}
