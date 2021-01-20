using FP.UoW.Synchronous;

using Moq;

using NUnit.Framework;

namespace FP.UoW.Tests
{
    public sealed class SynchronousUnitOfWorkControllerTest
    {
        [Test]
        public void Cannot_control_a_null_reference()
        {
            static void CreateControllerForNullReference()
            {
                SynchronousUnitOfWorkController.Wrap(null);
            }

            Assert.That(CreateControllerForNullReference, Throws.ArgumentNullException);
        }

        [Test]
        public void Creating_controller_begins_a_transaction()
        {
            var unitOfWorkMock = new Mock<ISynchronousUnitOfWork>();

            using var controller = SynchronousUnitOfWorkController.Wrap(unitOfWorkMock.Object);

            unitOfWorkMock.Verify(uow => uow.BeginTransaction(), Times.Once);
        }

        [Test]
        public void Disposing_controller_commits_a_transaction_if_not_Aborted()
        {
            var unitOfWorkMock = new Mock<ISynchronousUnitOfWork>();

            using (var controller = SynchronousUnitOfWorkController.Wrap(unitOfWorkMock.Object))
            {
                //Imagine that here things happen and the database is called
            }

            unitOfWorkMock.Verify(uow => uow.BeginTransaction(), Times.Once);
            unitOfWorkMock.Verify(uow => uow.CommitTransaction(), Times.Once);
        }

        [Test]
        public void Disposing_controller_does_nothing_if_a_transaction_was_Aborted()
        {
            var unitOfWorkMock = new Mock<ISynchronousUnitOfWork>();

            using (var controller = SynchronousUnitOfWorkController.Wrap(unitOfWorkMock.Object))
            {
                controller.Abort();
            }

            unitOfWorkMock.Verify(uow => uow.BeginTransaction(), Times.Once);
            unitOfWorkMock.Verify(uow => uow.RollbackTransaction(), Times.Once);
            unitOfWorkMock.Verify(uow => uow.CommitTransaction(), Times.Never);
        }
    }
}