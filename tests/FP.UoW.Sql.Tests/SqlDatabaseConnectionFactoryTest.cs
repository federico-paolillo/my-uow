using NUnit.Framework;

namespace FP.UoW.Sql.Tests
{
    public sealed class SqlDatabaseConnectionFactoryTest
    {
        [Test]
        public void Cannot_create_ConnectionFactory_without_a_ConnectionString()
        {
            static void CreateSqlDatabaseConnectionFactoryWithNullConnectionString()
            {
                _ = new SqlDatabaseConnectionFactory(null);
            }

            Assert.That(CreateSqlDatabaseConnectionFactoryWithNullConnectionString, Throws.ArgumentNullException);
        }
    }
}