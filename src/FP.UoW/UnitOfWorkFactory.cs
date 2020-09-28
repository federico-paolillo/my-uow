using System;
using System.Threading;
using System.Threading.Tasks;

namespace FP.UoW
{
    /// <inheritdoc/>
    internal sealed class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly IDatabaseConnectionFactory databaseConnectionFactory;

        public UnitOfWorkFactory(IDatabaseConnectionFactory databaseConnectionFactory)
        {
            this.databaseConnectionFactory = databaseConnectionFactory ?? throw new ArgumentNullException(nameof(databaseConnectionFactory));
        }

        /// <inheritdoc/>
        public Task<IUnitOfWork> MakeNewAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IUnitOfWork uow = new UnitOfWork(databaseConnectionFactory);

            return Task.FromResult(uow);
        }
    }
}
