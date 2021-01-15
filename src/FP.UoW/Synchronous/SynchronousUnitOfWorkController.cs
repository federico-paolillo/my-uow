using System;

namespace FP.UoW.Synchronous
{
    /// <summary>
    /// Wraps an <see cref="ISynchronousUnitOfWork"/> to reduce usage boilerplate.
    /// </summary>
    public sealed class SynchronousUnitOfWorkController : IDisposable
    {
        private readonly ISynchronousUnitOfWork unitOfWork;

        private bool commitOnDispose = true;

        private SynchronousUnitOfWorkController(ISynchronousUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <summary>
        /// Rollback the underlying <see cref="ISynchronousUnitOfWork"/>.
        /// </summary>
        public void Abort()
        {
            commitOnDispose = false;

            unitOfWork.RollbackTransaction();
        }

        public void Dispose()
        {
            if (commitOnDispose)
            {
                unitOfWork.CommitTransaction();
            }
        }

        /// <summary>
        /// Wraps a <see cref="ISynchronousUnitOfWork"/> and returns a <see cref="SynchronousUnitOfWorkController"/>.
        /// </summary>
        /// <param name="unitOfWork">The <see cref="ISynchronousUnitOfWork"/> to wrap.</param>
        /// <returns>A new <see cref="SynchronousUnitOfWorkController"/> that controls the <see cref="ISynchronousUnitOfWork"/> given.</returns>
        public static SynchronousUnitOfWorkController Wrap(ISynchronousUnitOfWork unitOfWork)
        {
            if (unitOfWork is null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }

            unitOfWork.BeginTransaction();

            var controller = new SynchronousUnitOfWorkController(unitOfWork);

            return controller;
        }
    }
}
