using FP.UoW.DependencyInjection;
using FP.UoW.Synchronous;

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
        public void If_ServiceCollection_instance_is_null_AddUow_throws()
        {
            static void TryAddUoWToNullReference()
            {
                _ = ServiceCollectionExtensions.AddUoW(null);
            }

            Assert.That(TryAddUoWToNullReference, Throws.ArgumentNullException);
        }

        [Test]
        public void If_UnitOfWorkServiceBuilder_instance_is_null_AddSynchronousImplementation_throws()
        {
            static void TryAddSynchronousImplementationToNullReference()
            {
                _ = ServiceCollectionExtensions.AddSynchronousImplementation(null);
            }

            Assert.That(TryAddSynchronousImplementationToNullReference, Throws.ArgumentNullException);
        }

        [TestCaseSource(typeof(ServiceLifetimesTestDataForAsynchronousImplementation), nameof(ServiceLifetimesTestDataForAsynchronousImplementation.Lifetimes))]
        public void Services_are_registered_as_expected(Type service, ServiceLifetime lifetime)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddUoW();

            var hasRegistration = serviceCollection
                .Any(descriptor => descriptor.ServiceType == service && descriptor.Lifetime == lifetime);

            Assert.That(hasRegistration, Is.True);
        }

        [Test]
        public void Services_can_be_resolved()
        {
            var databaseConnectionFactoryMock = new Mock<IDatabaseConnectionFactory>();

            var serviceCollection = new ServiceCollection();

            //Add Mocked IDatabaseConnectionFactory to ensure that services can be registered
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

        [Test]
        public void Synchronous_services_can_be_resolved()
        {
            var databaseConnectionFactoryMock = new Mock<IDatabaseConnectionFactory>();

            var serviceCollection = new ServiceCollection();

            //Add Mocked IDatabaseConnectionFactory to ensure that services can be registered
            serviceCollection.AddSingleton(databaseConnectionFactoryMock.Object);

            serviceCollection.AddUoW()
                .AddSynchronousImplementation();

            using var serviceProvider = serviceCollection.BuildServiceProvider();
            using var serviceScope = serviceProvider.CreateScope();

            var databaseSession = serviceScope.ServiceProvider.GetRequiredService<ISynchronousDatabaseSession>();
            var unitOfWork = serviceScope.ServiceProvider.GetRequiredService<SynchronousUnitOfWork>();
            var unitOfWorkInterface = serviceScope.ServiceProvider.GetRequiredService<ISynchronousUnitOfWork>();

            Assert.That(databaseSession, Is.Not.Null);
            Assert.That(unitOfWork, Is.Not.Null);
            Assert.That(unitOfWorkInterface, Is.Not.Null);
        }

        [TestCaseSource(typeof(ServiceLifetimesTestDataForSynchronousImplementation), nameof(ServiceLifetimesTestDataForSynchronousImplementation.Lifetimes))]
        public void Synchronous_services_are_registered_as_expected(Type service, ServiceLifetime lifetime)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddUoW()
                .AddSynchronousImplementation();

            var hasRegistration = serviceCollection
                .Any(descriptor => descriptor.ServiceType == service && descriptor.Lifetime == lifetime);

            Assert.That(hasRegistration, Is.True);
        }
    }

    internal sealed class ServiceLifetimesTestDataForAsynchronousImplementation
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

    internal sealed class ServiceLifetimesTestDataForSynchronousImplementation
    {
        public static IEnumerable Lifetimes
        {
            get
            {
                yield return new TestCaseData(typeof(SynchronousUnitOfWork), ServiceLifetime.Scoped);
                yield return new TestCaseData(typeof(ISynchronousUnitOfWork), ServiceLifetime.Scoped);
                yield return new TestCaseData(typeof(ISynchronousDatabaseSession), ServiceLifetime.Scoped);
            }
        }
    }

}