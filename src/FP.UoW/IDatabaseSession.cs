﻿using System.Data.Common;

namespace FP.UoW
{
    /// <summary>
    /// A database session. Contains the <see cref="DbConnection"/> and <see cref="DbTransaction" /> currently controlled by the Unit of Work.
    /// </summary>
    public interface IDatabaseSession
    {
        /// <summary>
        /// Current connection.
        /// Attempting to interact with the connection directly will result in undefined behavior.
        /// </summary>
        DbConnection Connection { get; }

        /// <summary>
        /// Current transaction.
        /// Attempting to interact with the transaction directly will result in undefined behavior.
        /// </summary>
        DbTransaction Transaction { get; }
    }
}
