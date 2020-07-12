using Autofac;
using System;

namespace FP.UoW.Autofac
{
    public sealed class UoWModule : Module
    {
        public string ConnectionString { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            if (builder is null) throw new ArgumentNullException(nameof(builder));

            if (string.IsNullOrWhiteSpace(ConnectionString)) throw new ArgumentException("No connection string was specified", nameof(ConnectionString));

            var databaseConnectionString = DatabaseConnectionString.From(ConnectionString);

            builder.RegisterInstance(databaseConnectionString)
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<UnitOfWork>()
                .As<IDatabaseUnitOfWork>()
                .As<IDatabaseSession>()
                .InstancePerLifetimeScope();
        }
    }
}
