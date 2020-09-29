using Autofac;

namespace FP.UoW.Autofac
{
    public sealed class UoWModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<UnitOfWorkFactory>()
                .As<IUnitOfWorkFactory>()
                .InstancePerDependency();

            builder.RegisterType<UnitOfWork>()
                .AsSelf()
                .As<IUnitOfWork>()
                .As<IDatabaseSession>()
                .InstancePerLifetimeScope();
        }
    }
}