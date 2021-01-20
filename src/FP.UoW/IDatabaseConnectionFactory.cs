using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace FP.UoW
{
    /// <summary>
    /// Builds new instances <see cref="DbConnection" /> to be used by the <see cref="IUnitOfWork" />.
    /// </summary>
    public interface IDatabaseConnectionFactory
    {
        /// <summary>
        /// Makes a new <see cref="DbConnection" /> for a particular Database.
        /// </summary>
        Task<DbConnection> MakeNewAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Makes a new <see cref="DbConnection" /> for a particular Database.
        /// </summary>
        DbConnection MakeNew();
    }
}