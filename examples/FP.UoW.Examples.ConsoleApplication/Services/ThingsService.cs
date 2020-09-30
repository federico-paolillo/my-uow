using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FP.UoW.Examples.ConsoleApplication.Models;
using FP.UoW.Examples.ConsoleApplication.Repositories;

namespace FP.UoW.Examples.ConsoleApplication.Services
{
    /// <summary>
    ///     Implements business logic for the Things model
    /// </summary>
    public sealed class ThingsService
    {
        private readonly ThingsRepository thingsRepository;
        private readonly IUnitOfWork unitOfWork;

        //We inject the IDatabaseUnitOfWork and the Repository. Usually you would use an interface for the repository, but to keep the example simple we just use the class
        public ThingsService(IUnitOfWork unitOfWork, ThingsRepository thingsRepository)
        {
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            this.thingsRepository = thingsRepository ?? throw new ArgumentNullException(nameof(thingsRepository));
        }

        public async Task InsertThingsAsync(int count, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                //Begin the Transaction, this implicitly Opens the connection
                await unitOfWork.BeginTransactionAsync(cancellationToken)
                    .ConfigureAwait(false);

                for (var i = 0; i < count; i++)
                {
                    var thing = new Thing
                    {
                        ColumnOne = Randomness.Number(),
                        ColumnTwo = Randomness.Number(),
                        ColumnThree = Randomness.Text()
                    };

                    //We do all our inserts for this Transaction...
                    await thingsRepository.InsertThingAsync(thing)
                        .ConfigureAwait(false);
                }

                //...and we complete the transaction which implicitly closes the Connection
                await unitOfWork.CommitTransactionAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch
            {
                //If something goes wrong we cancel the current transaction which implicitly closes the Connection
                await unitOfWork.RollbackTransactionAsync(cancellationToken)
                    .ConfigureAwait(false);

                //Irrelevant, but, we rethrow the exception because we caught it only to rollback the transaction.
                //The actual Exception will be handled elsewhere at an higher level
                throw;
            }
        }

        public async Task<IReadOnlyList<Thing>> GetThingsAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            //The service opens and closes the connection. Only the service is in charge of the connection lifetime.
            //Letting the service manage the connection lifetime will make sure that we can access multiple repositories or run multiple queries all within the same connection...
            //...this ensures that when a transaction is present all the repositories will use the same connection and transaction making them able to "see" any pending change.
            await unitOfWork.OpenConnectionAsync(cancellationToken)
                .ConfigureAwait(false);

            try
            {
                //We call the repository AFTER opening the connection, the repository does not manage the connection lifetime. 
                //The repository will access the connection through the IDatabaseSession injected into it therefore the connection must be open to use it.
                var entities = await thingsRepository.GetAllAsync()
                    .ConfigureAwait(false);

                //Usually you would add more processing to the entities (e.g.: values formatting), for this example we directly return the entities.
                //This makes this service layer borderline useless apart from the connection lifetime management, but for an example it is fine.
                return entities;
            }
            finally
            {
                await unitOfWork.CloseConnectionAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}