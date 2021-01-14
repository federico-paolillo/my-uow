using System;
using System.Threading;
using System.Threading.Tasks;

namespace FP.UoW
{
    /// <summary>
    /// Wraps an <see cref="IUnitOfWork" /> to reduce boilerplate
    /// </summary>
    public sealed class UnitOfWorkController : IDisposable
    {
        private readonly IUnitOfWork unitOfWork;

        private bool commitOnDispose = true;

        private UnitOfWorkController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async void Dispose()
        {
            if (commitOnDispose)
            {
                await unitOfWork.CommitTransactionAsync()
                    .ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        /// <summary>
        ///     Rollback the underlying Unit of Work
        /// </summary>
        public async Task AbortAsync(CancellationToken cancellationToken = default)
        {
            commitOnDispose = false;

            await unitOfWork.RollbackTransactionAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        ///     Wraps the <see cref="IUnitOfWork" /> provided and returns a new <see cref="UnitOfWorkController" />
        /// </summary>
        /// <param name="unitOfWorkToWrap">The <see cref="IUnitOfWork" /> to wrap</param>
        /// <returns>A new <see cref="UnitOfWorkController" /> controlling the <see cref="IUnitOfWork" /></returns>
        public static async Task<UnitOfWorkController> WrapAsync(IUnitOfWork unitOfWorkToWrap)
        {
            if (unitOfWorkToWrap == null)
            {
                throw new ArgumentNullException(nameof(unitOfWorkToWrap));
            }

            var controller = new UnitOfWorkController(unitOfWorkToWrap);

            // You can't really stop here. Wrap is kind of an "atomic" operation
            await unitOfWorkToWrap.BeginTransactionAsync(CancellationToken.None)
                .ConfigureAwait(false);

            return controller;
        }
    }
}