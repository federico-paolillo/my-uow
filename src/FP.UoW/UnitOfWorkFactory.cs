using System;
using System.Threading;
using System.Threading.Tasks;

namespace FP.UoW
{
    internal sealed class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private IDatabaseConnectionFactory databaseConnectionFactory;

        public UnitOfWorkFactory(IDatabaseConnectionFactory databaseConnectionFactory)
        {
            this.databaseConnectionFactory = databaseConnectionFactory ?? throw new ArgumentNullException(nameof(databaseConnectionFactory));
        }

        public Task<IDatabaseUnitOfWork> MakeNewAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IDatabaseUnitOfWork uow = new UnitOfWork(databaseConnectionFactory);

            return Task.FromResult(uow);
        }
    }
}
