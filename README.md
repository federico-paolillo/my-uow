# Unit of Work

Simple implementation of an Unit of Work.

## What is this ?

This is how I usually implement an Unit of Work, I do this often enough that I wanted to write it once.  
It is not perfect and it assumes a lot of things but it has proven to be effective enough to be useful.  

## Overview

The two projects needed to use the Unit of Work are:  

- `FP.UoW.Core` contains the `UnitOfWork` itself and all the required types to make it work.  
- `FP.UoW.Factories` contains database connection factories for MSSQL and SQLite.  

This package gives for granted that you are using some form of Dependency Injection in your Application.  
There are native extension methods for `Microsoft.Extensions.DependencyInjection` and an additional package for Autofac.  
Optionally, you can use the `UnitOfWork` without Dependency Injection but I don't recommend it, as it would made lifetime management hard.  

The two most important interfaces needed to work with `FP.UoW` are: `IDatabaseUnitOfWork` and `IDatabaseSession`.  

### `IDatabaseUnitOfWork` 

This interface exposes all the methods needed to manage the lifetime of the underlying connection and transaction.  
This interface should be injected in all those classes that invoke repository methods.  
Classes using this interface will be responsible to manage the connection, the transaction and to orchestrate any repository invocation.  

__Note:__ You are not required to always begin a transaction if you simply want to read some data from the database.

### `IDatabaseSession` 

This interface exposes the connection and transaction that is currently in use.  
This interface should be injected in a repository to give access to the underlying connection and transaction.  
Classes using this interface will not manage the connection and transaction.

__Note:__ Attempting to call methods on the connection and transaction object will result in undefined behavior.  
__Note:__ The connection and transaction might be null if nobody initialized them via `IDatabaseUnitOfWork`.

## Lifetime management

The idea behind `UnitOfWork` is to delegate the `UnitOfWork` lifetime to the Dependecy Injection container.  
The `UnitOfWork` is registered with a scoped lifetime, limiting one `UnitOfWork` instance for every request or command.  
Ensuring that the lifetime is managed by the Dependency Container lifetime scope effectively bounds one `UnitOfWork` to one request or command.  

The DI container will create an `UnitOfWork` instance bound to a lifetime scope then it will dispose of it when said lifetime scope ends.  
This ensures that User's code does not need to explicitly create or dispose the `UnitOfWork`. 

__Note:__ Calling code should only take care of opening and closing connection and committing and rollingback transactions.  
__Note:__ User code should not dispose the `UnitOfWork` manually

## Interface Segregation Principle

Although the `UnitOfWork` class is one, it implements different interfaces that provide different "views" to the Unit of Work pattern.  
`IDatabaseUnitOfWork` allows a service class to have access to the "controls" for the connection and the transaction lifetime.  
`IDatabaseSession` allows a repository class to have access to the current connection and transaction.  

This ensures that a class does not have access to methods or data that do not belong to its purpose, reducing mistakes. 
This splitting of interfaces effectively respects the Interface Segregation Principle where a class is not forced to depend on methods that it does not need.  

## Example

As said in the "Overview" section, the `IDatabaseUnitOfWork` should be used by a service-like class.  
A service-like class is usually responsible to "orchestrate" the business logic and call the repositories.  

Below is a method of a service-like class that setups the connection, transaction and orchestrates the repositories.  

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
Below, we see how a repository uses `IDatabaseSession` to perform its work but __DOES NOT__ attempt to influence the connection and transaction.  

```csharp
var query = @"
    INSERT INTO Things(Column_One, Column_Two, Column_Three)
    VALUES(@Column_One, @Column_Two, @Column_Three);
";

await databaseSession.Connection.ExecuteAsync(query, param: thing, transaction: databaseSession.Transaction)
    .ConfigureAwait(continueOnCapturedContext: false);
```

To have a better look at the whole picture refer to [the complete example](https://github.com/federico-paolillo/my-uow/tree/master/examples/FP.UoW.Examples.ConsoleApplication).  

You can run the example with the `list` command to see all the entries saved in the database.  
Use `insert --count <number>` to add random `<number>` records to the database.  

__Note:__ The example will create a SQLLite database file called `fp-uow-example.db` next to the executable.  
