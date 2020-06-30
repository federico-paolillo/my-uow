using System.Threading;
using System.Threading.Tasks;

namespace FP.UoW
{
    /// <summary>
    /// Exposes methods to interact with a database Connection, allowing to Open an Close a Connection
    /// </summary>
    public interface IDatabaseConnection
    {
        Task OpenAsync(CancellationToken cancellationToken = default);

        Task CloseAsync(CancellationToken cancellationToken = default);
    }
}
