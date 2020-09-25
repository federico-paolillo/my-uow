using System;
using System.Threading;
using System.Threading.Tasks;

namespace FP.UoW
{
    internal sealed class UnitOfWorkController : IDatabaseConnectionController, IUoWController
    {
        private readonly IDatabaseUnitOfWork unitOfWork;

        public UnitOfWorkController(IDatabaseUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public Task BeginAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task OpenAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        
        public void Dispose()
        {
            unitOfWork.CloseConnectionAsync()
        }
    }
}
