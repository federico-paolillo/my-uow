﻿namespace FP.UoW
{
    /// <summary>
    /// Options that configure the <see cref="IUnitOfWork"/> behavior
    /// </summary>
    public sealed class UnitOfWorkOptions
    {
        public static readonly UnitOfWorkOptions Default = new()
        {
            ThrowOnMultipleConnectionsAttempts = true,
            ThrowOnMultipleTransactionsAttempts = true
        };

        /// <summary>
        /// Disable exceptions when trying to open an new database connection when one is already open.
        /// This merely stops the exception from being thrown, a new connection will NOT be opened.
        /// </summary>
        public bool ThrowOnMultipleConnectionsAttempts { get; init; }

        /// <summary>
        /// Disable exceptions when trying to begin an new database transaction when one is already running.
        /// This merely stops the exception from being thrown, a new transaction will NOT begin. 
        /// </summary>
        public bool ThrowOnMultipleTransactionsAttempts { get; init; }
    }
}
