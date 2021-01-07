using FP.UoW.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

using System;
using System.Collections;
using System.Linq;

namespace FP.UoW.Tests
{
    public sealed class ServiceCollectionExtensionsTest
    {
        [Test]
        public void Cannot_create_UnitOfWorkServiceBuilder_without_a_ServiceCollection()
        {
            static void TryCreateUnitOfWorkConnectionBuilderOnNullServiceCollection()
            {
                ServiceCollection serviceCollection = null;

                serviceCollection.AddUoW();
            }

            Assert.That(TryCreateUnitOfWorkConnectionBuilderOnNullServiceCollection, Throws.ArgumentNullException);
        }

        [Test]
        public void If_ServiceCollection_instance_is_null_throws()
        {
            static void TryAddUoWToNullReference()
            {
                ServiceCollectionExtensions.AddUoW(null);
            }

            Assert.That(TryAddUoWToNullReference, Throws.ArgumentNullException);
        }

        [TestCaseSource(typeof(ServiceLifetimesTestData), nameof(ServiceLifetimesTestData.Lifetimes))]
        public void Services_are_registered_as_expected(Type service, ServiceLifetime lifetime)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddUoW();

            var hasRegistration = serviceCollection.Any(descriptor =>
                descriptor.ServiceType == service && descriptor.Lifetime == lifetime);

            Assert.That(hasRegistration, Is.True);
        }

        [Test]
        public void Services_can_be_resolved()
        {
            var databaseConnectionFactoryMock = new Mock<IDatabaseConnectionFactory>();

            var serviceCollection = new ServiceCollection();

            //Add Mocked IDatabaseConnectionFactory to ensure that services can indeed be registered
            serviceCollection.AddSingleton(databaseConnectionFactoryMock.Object);
            serviceCollection.AddUoW();

            using var serviceProvider = serviceCollection.BuildServiceProvider();
            using var serviceScope = serviceProvider.CreateScope();

            var databaseSession = serviceScope.ServiceProvider.GetRequiredService<IDatabaseSession>();
            var unitOfWork = serviceScope.ServiceProvider.GetRequiredService<UnitOfWork>();
            var unitOfWorkInterface = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            Assert.That(databaseSession, Is.Not.Null);
            Assert.That(unitOfWork, Is.Not.Null);
            Assert.That(unitOfWorkInterface, Is.Not.Null);
        }
    }

    internal sealed class ServiceLifetimesTestData
    {
        public static IEnumerable Lifetimes
        {
            get
            {
                yield return new TestCaseData(typeof(UnitOfWork), ServiceLifetime.Scoped);
                yield return new TestCaseData(typeof(IUnitOfWork), ServiceLifetime.Scoped);
                yield return new TestCaseData(typeof(IDatabaseSession), ServiceLifetime.Scoped);
            }
        }
    }
}