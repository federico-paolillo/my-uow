using System;
using System.Collections;
using System.Linq;
using FP.UoW.DependencyInjection;
using FP.UoW.Sql.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FP.UoW.Sql.Tests
{
    public class UoWServiceBuilderExtensionsTest
    {
        [Test]
        public void If_UoWServiceBuilder_instance_is_null_throws()
        {
            void TryAddSqlToNullReference()
            {
                UoWServiceBuilderExtensions.ForSql(null, "random_connection_string_it_does_not_matter");
            }

            Assert.That(TryAddSqlToNullReference, Throws.ArgumentNullException);
        }

        [Test]
        public void If_Connection_String_is_null_throws()
        {
            void TryAddSqlWithoutConnectionString()
            {
                var serviceCollection = new ServiceCollection();

                serviceCollection.AddUoW()
                    .ForSql(null);
            }

            Assert.That(TryAddSqlWithoutConnectionString, Throws.ArgumentException);
        }

        [TestCaseSource(typeof(ServiceLifetimesTestData), nameof(ServiceLifetimesTestData.Lifetimes))]
        public void Services_are_registered_as_expected(Type service, ServiceLifetime lifetime)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddUoW()
                .ForSql("random_connection_string_it_does_not_matter");

            var hasRegistration = serviceCollection.Any(descriptor =>
                descriptor.ServiceType == service && descriptor.Lifetime == lifetime);

            Assert.That(hasRegistration, Is.True);
        }

        [Test]
        public void Services_can_be_resolved()
        {
            var serviceCollection = new ServiceCollection();

            //Add Mocked IDatabaseConnectionFactory to ensure that services can indeed be registered
            serviceCollection.AddUoW()
                .ForSql("random_connection_string_it_does_not_matter");

            using var serviceProvider = serviceCollection.BuildServiceProvider();
            using var serviceScope = serviceProvider.CreateScope();

            var databaseSession = serviceScope.ServiceProvider.GetRequiredService<IDatabaseSession>();
            var unitOfWork = serviceScope.ServiceProvider.GetRequiredService<UnitOfWork>();
            var unitOfWorkInterface = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var connectionFactory = serviceScope.ServiceProvider.GetRequiredService<IDatabaseConnectionFactory>();
            var connectionString = serviceScope.ServiceProvider.GetRequiredService<SqlDatabaseConnectionString>();

            Assert.That(databaseSession, Is.Not.Null);
            Assert.That(unitOfWork, Is.Not.Null);
            Assert.That(unitOfWorkInterface, Is.Not.Null);
            Assert.That(connectionFactory, Is.Not.Null);
            Assert.That(connectionString, Is.Not.Null);
        }
    }

    internal sealed class ServiceLifetimesTestData
    {
        public static IEnumerable Lifetimes
        {
            get
            {
                yield return new TestCaseData(typeof(IDatabaseConnectionFactory), ServiceLifetime.Transient);
                yield return new TestCaseData(typeof(SqlDatabaseConnectionString), ServiceLifetime.Singleton);
            }
        }
    }
}