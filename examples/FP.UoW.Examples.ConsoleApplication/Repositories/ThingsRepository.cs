using Dapper;
using FP.UoW.Examples.ConsoleApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FP.UoW.Examples.ConsoleApplication.Repositories
{
    public sealed class ThingsRepository
    {
        private readonly IDatabaseSession databaseSession = null;

        public ThingsRepository(IDatabaseSession databaseSession)
        {
            this.databaseSession = databaseSession ?? throw new ArgumentNullException(nameof(databaseSession));
        }

        public async Task<IReadOnlyList<Thing>> GetAllAsync()
        {
            var query = @"
                SELECT * FROM Things;
            ";

            //We use the database session here to run our queries.
            //Passing the transaction is not needed for a readonly query and it will be null...
            //...although I find that it is better to always pass it so that you never forget.
            var things = await databaseSession.Connection.QueryAsync<Thing>(query, transaction: databaseSession.Transaction)
                .ConfigureAwait(continueOnCapturedContext: false);

            return things.ToList();
        }

        public async Task InsertThingAsync(Thing thing)
        {
            if (thing is null) throw new ArgumentNullException(nameof(thing));

            var query = @"
                INSERT INTO Things(Column_One, Column_Two, Column_Three)
                VALUES(@Column_One, @Column_Two, @Column_Three);
            ";

            await databaseSession.Connection.ExecuteAsync(query, param: thing, transaction: databaseSession.Transaction)
                .ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Creates the database tables if needed, not very important for the example.
        /// </summary>
        public async Task EnsureTableCreatedAsync()
        {
            var query = @"
                CREATE TABLE IF NOT EXISTS Things (

                    Column_One INT PRIMARY KEY,
                    Column_Two INT NOT NULL,
                    Column_Three TEXT NOT NULL
                );
            ";

            await databaseSession.Connection.ExecuteAsync(query, transaction: databaseSession.Transaction)
                .ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}
