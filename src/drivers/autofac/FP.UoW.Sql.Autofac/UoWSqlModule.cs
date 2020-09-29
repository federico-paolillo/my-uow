using System;
using Autofac;
using FP.UoW.Factories;

namespace FP.UoW.Sql.Autofac
{
    public sealed class UoWSqlModule : Module
    {
        public string DatabaseConnectionString { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            if (string.IsNullOrWhiteSpace(DatabaseConnectionString))
                throw new ArgumentException("You must specify a Database Connection String",
                    nameof(DatabaseConnectionString));

            var sqlDatabaseConnectionString = SqlDatabaseConnectionString.From(DatabaseConnectionString);

            builder.RegisterInstance(sqlDatabaseConnectionString)
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<SqlDatabaseConnectionFactory>()
                .As<IDatabaseConnectionFactory>()
                .InstancePerDependency();
        }
    }
}