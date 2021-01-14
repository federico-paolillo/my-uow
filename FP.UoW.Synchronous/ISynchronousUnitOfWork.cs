namespace FP.UoW.Synchronous
{
    /// <summary>
    /// Exposes methods to interact with an Unit of Work, allowing to Begin, Rollback or Commit an Unit of Work. Synchronously.
    /// </summary>
    public interface ISynchronousUnitOfWork : IDatabaseSession
    {
        /// <summary>
        /// Opens a connection to the database
        /// </summary>
        void OpenConnection();

        /// <summary>
        /// Close the connection to the database
        /// </summary>
        void CloseConnection();

        /// <summary>
        /// Begin a transaction using the open connection
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Rollbacks the current transaction, removing any changes from the database
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        /// Commits the current transaction, persisting changes to the database
        /// </summary>
        void CommitTransaction();
    }
}
