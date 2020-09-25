using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace FP.UoW
{
    /// <summary>
    /// Builds a <see cref="DbConnection"/> to be used by the <see cref="IDatabaseUnitOfWork"/>
    /// </summary>
    public interface IDatabaseConnectionFactory
    {
        Task<DbConnection> MakeNewAsync(CancellationToken cancellationToken = default);
    }
}
