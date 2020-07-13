using CliFx;
using FP.UoW.Examples.ConsoleApplication.Commands;
using FP.UoW.Examples.ConsoleApplication.Repositories;
using FP.UoW.Examples.ConsoleApplication.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace FP.UoW.Examples.ConsoleApplication
{
    public sealed class Program
    {
        private const string CONNECTION_STRING = "Data Source = fp-uow-example.db";

        public static async Task Main(string[] args)
        {
            //Here we add the SQLite UnitOfWork implementation the ServiceCollection
            var serviceCollection = new ServiceCollection()
                .AddSQLiteUoW(CONNECTION_STRING);

            //We register some services and repositories that will be used in the example to access the db
            serviceCollection.AddTransient<ThingsService>();
            serviceCollection.AddTransient<ThingsRepository>();

            serviceCollection.AddTransient<ListCommand>();
            serviceCollection.AddTransient<InsertCommand>();

            //We setup the service provider and a new service provider scope
            //Consider a service provider scope roughly like a transaction for dependency injection
            using var serviceProvider = serviceCollection.BuildServiceProvider();
            using var servicesScope = serviceProvider.CreateScope();

            //Irrelevant, third party code to start up the application
            //Read ListCommand and InsertCommand for the next steps
            var application = new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .UseTypeActivator(type => servicesScope.ServiceProvider.GetRequiredService(type))
                .Build();

            await application.RunAsync()
                .ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}
