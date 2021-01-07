using NUnit.Framework;

namespace FP.UoW.SQLite.Tests
{
    public sealed class SQLiteDatabaseConnectionFactoryTest
    {
        [Test]
        public void Cannot_create_ConnectionFactory_without_a_ConnectionString()
        {
            static void CreateSQLiteDatabaseConnectionFactoryWithNullConnectionString()
            {
                _ = new SQLiteDatabaseConnectionFactory(null);
            }

            Assert.That(CreateSQLiteDatabaseConnectionFactoryWithNullConnectionString, Throws.ArgumentNullException);
        }
    }
}