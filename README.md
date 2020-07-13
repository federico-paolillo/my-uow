# Unit of Work

Simple implementation of an Unit of Work.

## What is this ?

This is how I usually implement the Unit of Work pattern, I do this often enough that I wanted to write it once and use it anywhere.  
It is not perfect and gives for granted quite a lot of thing but it has shown to be effective enough to be useful.  

## Usage

There are two main projects needed to use the Unit of Work:  

- `FP.UoW.Core` contains the `UnitOfWork` itself and all the required types to make it work.  
- `FP.UoW.Factories` contains database connection factories for MSSQL and SQLite.  

The `UnitOfWork` gives for granted that you are using some form of Dependency Injection container and exposes extension methods for `Microsoft.Extensions.DependencyInjection` DI container.  
It is possible to use the `UnitOfWork` without any DI container, although discouraged as it would require precise manual lifetime management.  

There are two interfaces to keep in mind while working with `FP.UoW`: `IDatabaseUnitOfWork` and `IDatabaseSession`.  

`IDatabaseUnitOfWork` exposes all the methods needed to manage the lifetime of the underlying connection and transaction.  
This interface should be injected in a service-like class or a controller method.  
Users of this interface will be responsible to manage the connection and transaction lifetime and will orchestrate the various repositories.  

`IDatabaseSession` exposes two properties, one for the current connection and one for the current transaction.  
This interface should be injected in a repository class to give access to the underlying connection and transaction.  
Users of this interface will not be responsible to manage the connection and transaction lifetime.  
Attempting to call methods on the exposed connection and transaction properties will result in undefined behavior.  

## Example

To summarize the "Usage" section, the `IDatabaseUnitOfWork` should be used by a class responsible to "orchestrate" the business logic and call the repositories.  
Below a fragment of a method of such a class that setups the connection and transaction and orchestrate the repositories.  

```csharp
try
{
    //We open the connection, the service always handle the connection and transaction lifetime
    await unitOfWork.OpenConnectionAsync(cancellationToken)
        .ConfigureAwait(continueOnCapturedContext: false);

    //Then we start the transaction
    await unitOfWork.BeginTransactionAsync(cancellationToken)
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
```

The repositories, instead, should use the `IDatabaseSession` to access the current connection and transaction.  
In the method below, we can see how the repository uses the connection and transaction to perform its work but DOES NOT in any way attempt to influence the connection and transaction.  

```csharp
var query = @"
    INSERT INTO Things(Column_One, Column_Two, Column_Three)
    VALUES(@Column_One, @Column_Two, @Column_Three);
";

await databaseSession.Connection.ExecuteAsync(query, param: thing, transaction: databaseSession.Transaction)
    .ConfigureAwait(continueOnCapturedContext: false);
```

To have a better look at the whole picture refer to `src/examples/FP.UoW.Examples.ConsoleApplication` for the complete example.
You can run the example with `list` switch to see all the entries saved in the database.  
Use `insert --count <number>` to add random `<number>` records to the database.  
The example will create a file called `fp-uow-example.db` next to the executable.  