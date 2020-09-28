using System.Threading;
using System.Threading.Tasks;

namespace FP.UoW
{
    /// <summary>
    /// Builds new instances of <see cref="IUnitOfWork"/> not bound to any scope
    /// </summary>
    public interface IUnitOfWorkFactory
    {
        /// <summary>
        /// Makes a new <see cref="IUnitOfWork"/> not bound to any scope
        /// </summary>
        Task<IUnitOfWork> MakeNewAsync(CancellationToken cancellationToken = default);
    }
}
