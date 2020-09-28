using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace FP.UoW
{
    /// <summary>
    /// Builds new instances <see cref="DbConnection"/> to be used by the <see cref="IUnitOfWork"/> not bound to any scope
    /// </summary>
    public interface IDatabaseConnectionFactory
    {
        /// <summary>
        /// Makes a new <see cref="DbConnection"/> not bound to any scope
        /// </summary>
        Task<DbConnection> MakeNewAsync(CancellationToken cancellationToken = default);
    }
}
