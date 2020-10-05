namespace FP.UoW.Sql.Tests.Infrastructure
{
    public sealed class TestModel
    {
        private TestModel()
        {
        }

        public int Id { get; private set; }

        public string ColumnOne { get; private set; }

        public string ColumnTwo { get; private set; }

        public static TestModel Random()
        {
            return new TestModel
            {
                Id = Randomness.Number(),
                ColumnOne = Randomness.Text(),
                ColumnTwo = Randomness.Text()
            };
        }
    }
}