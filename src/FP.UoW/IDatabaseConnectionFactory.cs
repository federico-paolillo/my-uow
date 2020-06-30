using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace FP.UoW
{
    public interface IDatabaseConnectionFactory
    {
        Task<DbConnection> MakeDatabaseConnectionAsync(CancellationToken cancellationToken = default);
    }
}
