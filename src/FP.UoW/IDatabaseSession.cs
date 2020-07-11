using System.Data.Common;

namespace FP.UoW
{
    /// <summary>
    /// A database session. Contains the current <see cref="DbConnection"/> and <see cref="DbTransaction" /> in use
    /// </summary>
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
