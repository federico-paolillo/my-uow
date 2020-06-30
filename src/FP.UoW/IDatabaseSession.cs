using System.Data.Common;

namespace FP.UoW
{
    /// <summary>
    /// A database session data. Contains the current IDbConnection and IDbTransaction in use
    /// </summary>
    public interface IDatabaseSession
    {
        /// <summary>
        /// Current Connection, attempting to alter the Connection state directly is a mistake
        /// </summary>
        DbConnection Connection { get; }

        /// <summary>
        /// Current Transaction, attempting to alter the Transaction state directly is a mistake
        /// </summary>
        DbTransaction Transaction { get; }
    }
}
