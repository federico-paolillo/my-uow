using FP.UoW.Examples.ConsoleApplication.Models;
using FP.UoW.Examples.ConsoleApplication.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FP.UoW.Examples.ConsoleApplication.Services
{
    /// <summary>
    /// Implements business logic for the Things model
    /// </summary>
    public sealed class ThingsService
    {
        private readonly IDatabaseUnitOfWork unitOfWork = null;
        private readonly ThingsRepository thingsRepository = null;

        //We inject the IDatabaseUnitOfWork and the Repository. Usually you would use an interface for the repository, but to keep the example simple we just use the class
        public ThingsService(IDatabaseUnitOfWork unitOfWork, ThingsRepository thingsRepository)
        {
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            this.thingsRepository = thingsRepository ?? throw new ArgumentNullException(nameof(thingsRepository));
        }

        public async Task InsertThingsAsync(int count, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                //We open the connection, the service always handle the connection and transaction lifetime
                await unitOfWork.OpenConnectionAsync(cancellationToken)
                    .ConfigureAwait(continueOnCapturedContext: false);

                //Then we start the transaction
                await unitOfWork.BeginTransactionAsync(cancellationToken)
                    .ConfigureAwait(continueOnCapturedContext: false);

                //Creates the database table in case they don't exist, so that we have database file to work with
                await thingsRepository.EnsureTableCreatedAsync()
                    .ConfigureAwait(continueOnCapturedContext: false);

                for (int i = 0; i < count; i++)
                {
                    var thing = new Thing
                    {
                        Column_One = Randomness.Number(),
                        Column_Two = Randomness.Number(),
                        Column_Three = Randomness.Text()
                    };

                    //We do all our inserts
                    await thingsRepository.InsertThingAsync(thing)
                        .ConfigureAwait(continueOnCapturedContext: false);
                }

                //And we complete the transaction
                await unitOfWork.CommitTransactionAsync(cancellationToken)
                    .ConfigureAwait(continueOnCapturedContext: false);
            }
            catch
            {
                //If something goes wrong (even a Cancellation) we rollback everything
                await unitOfWork.RollbackTransactionAsync(cancellationToken)
                    .ConfigureAwait(continueOnCapturedContext: false);

                //Irrelevant, but, we rethrow the exception because we catch it only to rollback the transaction and not to handle the transaction itself
                throw;
            }
            finally
            {
                //Always remember to close the connection
                await unitOfWork.CloseConnectionAsync(cancellationToken)
                    .ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        public async Task<IReadOnlyList<Thing>> GetThingsAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            //The service opens and closes the connection. Only the service is in charge of the connection lifetime.
            //Letting the service manage the connection lifetime will make sure that we can access multiple repositories or run multiple queries all within the same connection...
            //...this ensures that when a transaction is present all the repositories will use the same connection and transaction making them able to "see" any pending change.
            await unitOfWork.OpenConnectionAsync(cancellationToken)
                .ConfigureAwait(continueOnCapturedContext: false);

            try
            {
                //Creates the database table in case they don't exist, so that we have database file to work with
                await thingsRepository.EnsureTableCreatedAsync()
                    .ConfigureAwait(continueOnCapturedContext: false);

                //We call the repository AFTER opening the connection, the repository does not manage the connection lifetime. 
                //The repository will access the connection through the IDatabaseSession injected into it therefore the connection must be open to use it.
                var entities = await thingsRepository.GetAllAsync()
                    .ConfigureAwait(continueOnCapturedContext: false);

                //Usually you would add more processing to the entities (e.g.: values formatting), for this example we directly return the entities.
                //This makes this service layer borderline useless apart from the connection lifetime management, but for an example it is fine.
                return entities;
            }
            finally
            {
                await unitOfWork.CloseConnectionAsync()
                    .ConfigureAwait(continueOnCapturedContext: false);
            }
        }
    }
}
