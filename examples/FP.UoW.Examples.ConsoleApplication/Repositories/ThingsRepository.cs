using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FP.UoW.Examples.ConsoleApplication.Models;

namespace FP.UoW.Examples.ConsoleApplication.Repositories
{
    public sealed class ThingsRepository
    {
        private readonly IDatabaseSession databaseSession;

        public ThingsRepository(IDatabaseSession databaseSession)
        {
            this.databaseSession = databaseSession ?? throw new ArgumentNullException(nameof(databaseSession));
        }

        public async Task<IReadOnlyList<Thing>> GetAllAsync()
        {
            await EnsureTableCreatedAsync()
                .ConfigureAwait(false);

            var query = @"
                SELECT * FROM Things;
            ";

            //We use the database session here to run our queries.
            //Passing the transaction is not needed for a readonly query and it will be null...
            //...although I find that it is better to always pass it so that you never forget.
            var things = await databaseSession.Connection
                .QueryAsync<Thing>(query, transaction: databaseSession.Transaction)
                .ConfigureAwait(false);

            return things.ToList();
        }

        public async Task InsertThingAsync(Thing thing)
        {
            if (thing is null) throw new ArgumentNullException(nameof(thing));

            await EnsureTableCreatedAsync()
                .ConfigureAwait(false);

            var query = @"
                INSERT INTO Things(ColumnOne, ColumnTwo, ColumnThree)
                VALUES(@ColumnOne, @ColumnTwo, @ColumnThree);
            ";

            await databaseSession.Connection.ExecuteAsync(query, thing, databaseSession.Transaction)
                .ConfigureAwait(false);
        }

        /// <summary>
        ///     Creates the database tables if needed, not very important for the example.
        /// </summary>
        private async Task EnsureTableCreatedAsync()
        {
            var query = @"
                CREATE TABLE IF NOT EXISTS Things (

                    ColumnOne INT PRIMARY KEY,
                    ColumnTwo INT NOT NULL,
                    ColumnThree TEXT NOT NULL
                );
            ";

            await databaseSession.Connection.ExecuteAsync(query, transaction: databaseSession.Transaction)
                .ConfigureAwait(false);
        }
    }
}