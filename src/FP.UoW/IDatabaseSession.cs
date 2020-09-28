using System.Data.Common;

namespace FP.UoW
{
    /// <summary>
    /// A database session. Contains the <see cref="DbConnection"/> and <see cref="DbTransaction" /> currently in use for the scope
    /// </summary>
    /// <remarks>When injected will refer to the current DI scope</remarks>
    public interface IDatabaseSession
    {
        /// <summary>
        /// Current connection, attempting to alter the connection state directly will result in an undefined behavior
        /// </summary>
        DbConnection Connection { get; }

        /// <summary>
        /// Current transaction, attempting to alter the transaction state directly will result in an undefined behavior
        /// </summary>
        DbTransaction Transaction { get; }
    }
}
