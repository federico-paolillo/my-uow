using Autofac;
using FP.UoW.Autofac;
using System;

namespace FP.UoW.Factories.Autofac
{
    public sealed class UoWSQLiteModule : Module
    {
        public string ConnectionString { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            if (builder is null) throw new ArgumentNullException(nameof(builder));

            if (string.IsNullOrWhiteSpace(ConnectionString)) throw new ArgumentException("No connection string was specified", nameof(ConnectionString));

            builder.RegisterModule(new UoWModule { ConnectionString = ConnectionString });

            builder.RegisterType<IDatabaseConnectionFactory>()
                .As<SQLiteConnectionFactory>()
                .InstancePerDependency();
        }
    }
}
