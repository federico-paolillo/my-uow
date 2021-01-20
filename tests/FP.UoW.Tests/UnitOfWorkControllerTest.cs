using Moq;

using NUnit.Framework;

using System.Threading;
using System.Threading.Tasks;

namespace FP.UoW.Tests
{
    public sealed class UnitOfWorkControllerTest
    {
        [Test]
        public void Cannot_control_a_null_reference()
        {
            static async Task CreateControllerForNullReference()
            {
                await UnitOfWorkController.WrapAsync(null)
                    .ConfigureAwait(false);
            }

            Assert.That(CreateControllerForNullReference, Throws.ArgumentNullException);
        }

        [Test]
        public async Task Creating_controller_begins_a_transaction()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            using var controller = await UnitOfWorkController.WrapAsync(unitOfWorkMock.Object)
                .ConfigureAwait(false);

            unitOfWorkMock.Verify(uow => uow.BeginTransactionAsync(CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task Disposing_controller_commits_a_transaction_if_not_Aborted()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            using (var controller = await UnitOfWorkController.WrapAsync(unitOfWorkMock.Object)
                .ConfigureAwait(false))
            {
                //Imagine that here things happen and the database is called
            }

            unitOfWorkMock.Verify(uow => uow.BeginTransactionAsync(CancellationToken.None), Times.Once);
            unitOfWorkMock.Verify(uow => uow.CommitTransactionAsync(CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task Disposing_controller_does_nothing_if_a_transaction_was_Aborted()
        {
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            using (var controller = await UnitOfWorkController.WrapAsync(unitOfWorkMock.Object)
                .ConfigureAwait(false))
            {
                await controller.AbortAsync()
                    .ConfigureAwait(false);
            }

            unitOfWorkMock.Verify(uow => uow.BeginTransactionAsync(CancellationToken.None), Times.Once);
            unitOfWorkMock.Verify(uow => uow.RollbackTransactionAsync(CancellationToken.None), Times.Once);
            unitOfWorkMock.Verify(uow => uow.CommitTransactionAsync(CancellationToken.None), Times.Never);
        }
    }
}