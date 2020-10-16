# FP.UoW

An Unit of Work implementation that simplifies lifetime management of connection and transactions.  

## Project overview

### FP.UoW

The main project is, unsurprisingly, `FP.UoW`.  
This project provides all the classes needed to use the library.  
There are no dependencies on any IoC container or other library not defined in .NET Standard 2.1.  

#### `UnitOfWork`

This class exposes methods to manage the underlying database connection and transaction.  
Once you open a connection and, **optionally**, begin a transaction the Unit of Work is ready to be used.  
You can access the `Connection` and `Transaction` properties to execute your database queries.  
When you work is done remember to close any database resource using the methods provided on the `UnitOfWork`.  
It is possible to use an `UnitOfWork` multiple times without the need to create a new one every time.  

**Note**: `UnitOfWork` is not thread safe, create a new `UnitOfWork` for every thread.  
**Note**: `UnitOfWork` can only have one connection and one transaction running at a time.  

To reduce boilerplate, any time you begin a transaction a new connection is opened for you automatically.  
Conversely, when committing or rolling-back a transaction, the connection is closed for you automatically.  
If you want to further reduce boilerplate have a look at `UnitOfWorkController`.  

**Note:** Only use the methods on the `UnitOfWork` to alter the connection and transaction exposed.  
**Note:** You can open a connection without necessarily beginning a transaction (useful when just reading).   

_Remark:_ Exposing `Connection` and `Transaction` violate encapsulation but it is the most effective to make them accessible.  

You might have noticed that `UnitOfWork` implements two interfaces: `IUnitOfWork` and `IDatabaseSession`.  
These two interfaces are meant to provide a different 'view' to the Unit of Work to different actors and promotes [SRP](https://en.wikipedia.org/wiki/Single-responsibility_principle).  
`IDatabaseSession` is meant to be referenced by those classes that only need to access the current connection and transaction.  
`IUnitOfWork` is meant for those classes that manage the lifetime of the current connection and transaction.  
This separation ensures a clear cut separation of concerns between the two use cases described above.  
In general, your repositories will depend on `IDatabaseSession` and your services, controllers, etc... will depend on `IUnitOfWork`.  

_Remark:_ It is not mandatory to separate your Unit of Work usage with `IDatabaseSession` and `IUnitOfWork`.

#### `IDatabaseUnitOfWork`

To make a new Unit of Work you need an implementation of this interface.  
Implementations of `IDatabaseUnitOfWork` provide means for the Unit of Work to get new database connections to use.  

**Note:** There are two factories provided for you for SQLite and Microsoft SQL Server in their respective assembly.  

_Remark_: Don't use this interface directly, to just open a connection call the appropriate method on the Unit Of Work.  

#### `UnitOfWorkController`

To avoid too much boilerplate when using transactions you can use `UnitOfWorkController`.  
This class abuses the `IDisposable` pattern to automatically begin and commit transactions for you.  
To learn more, see the Example section that shows how this class can improve readability

### FP.UoW.DependencyInjection

This assembly contains extensions methods to wire an `UnitOfWork` with scoped lifetime to `Microsoft.Extensions.DependencyInjection` DI container.  
You must combine this assembly with one of the database integrations to be able to actually use an `UnitOfWork` from the container.  

### FP.UoW.SQLite.*

These assemblies contains the integration for SQLite, that is: an implementation of `IDatabaseConnectionFactory` for SQLite.
These assemblies also provide provide various integrations for different DI containers.

### FP.UoW.Sql.*

These assemblies contains the integration for Microsoft SQL Server, that is: an implementation of `IDatabaseConnectionFactory` for Microsoft SQL Server.
These assemblies also provide provide various integrations for different DI containers.

## Example

**Note:** The examples use the async version of the methods. Sync methods work the same.  

### Explicit use of `UnitOfWork`

Let's say you want to insert some records in your SQLite database:

```c#

var connection = SQLiteDatabaseConnectionString.From("<yourconnectionstringhere>");
var connectionFactory = new SQLiteDatabaseConnectionFactory(connection);
using var unitOfWork = new UnitOfWork(connectionFactory);

try {

    await unitOfWork.OpenConnectionAsync()
        .ConfigureAwait(false);

    await unitOfWork.BeginTransactionAsync()
        .ConfigureAwait(false);

    //unitOfWork.Connection.ExecuteAsync... or whatever you use to execute queries

    await unitOfWork.CommitTransactionAsync()
        .ConfigureAwait(false);

}
catch {

    await unitOfWork.RollbackTransactionAsync()
        .ConfigureAwait(false);

}
finally {

    await unitOfWork.CloseConnectionAsync()
        .ConfigureAwait(false);

}

```

Now we just want to read some records without making changes:

```c#
var connection = SQLiteDatabaseConnectionString.From("<yourconnectionstringhere>");
var connectionFactory = new SQLiteDatabaseConnectionFactory(connection);
using var unitOfWork = new UnitOfWork(connectionFactory);

try {

    await unitOfWork.OpenConnectionAsync()
        .ConfigureAwait(false);

    //unitOfWork.Connection.ExecuteAsync... or whatever you use to execute queries

}
finally {

    await unitOfWork.CloseConnectionAsync()
        .ConfigureAwait(false);

}

```
### Implicit connection management

Opening and closing the connection is boring, luckily `Begin/Commit/RollbackTransactionAsync` do that automatically.  
Let's say you want to insert some records in your SQLite database:

```c#

var connection = SQLiteDatabaseConnectionString.From("<yourconnectionstringhere>");
var connectionFactory = new SQLiteDatabaseConnectionFactory(connection);
using var unitOfWork = new UnitOfWork(connectionFactory);

try {

    await unitOfWork.BeginTransactionAsync()
        .ConfigureAwait(false);

    //unitOfWork.Connection.ExecuteAsync... or whatever you use to execute queries

    await unitOfWork.CommitTransactionAsync()
        .ConfigureAwait(false);

}
catch {

    await unitOfWork.RollbackTransactionAsync()
        .ConfigureAwait(false);

}

```

**Note:** Reading records is the same as the example above.

### Using `UnitOfWorkController`

To reduce boilerplate even further and forget about managing transactions we can leverage `UnitOfWorkController`.
Let's say you want to insert some records in your SQLite database:

```c#

var connection = SQLiteDatabaseConnectionString.From("<yourconnectionstringhere>");
var connectionFactory = new SQLiteDatabaseConnectionFactory(connection);
using var unitOfWork = new UnitOfWork(connectionFactory);
using var unitOfWorkController = new UnitOfWorkController(unitOfWork);

try {

    //unitOfWork.Connection.ExecuteAsync... or whatever you use to execute queries

}
catch {

    await unitOfWorkController.AbortAsync()
        .ConfigureAwait(false);
    
}

```

**Note:** Reading records is the same as the example above.

-- TODO --

## Dependency Injection

### Lifetime management

### Supported DI Containers

