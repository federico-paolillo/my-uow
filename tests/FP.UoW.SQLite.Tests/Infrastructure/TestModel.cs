namespace FP.UoW.SQLite.Tests.Infrastructure
{
    public sealed class TestModel
    {
        private TestModel()
        {
        }

        public string Id { get; private set; }

        public string ColumnOne { get; private set; }

        public string ColumnTwo { get; private set; }

        public static TestModel Random()
        {
            return new TestModel
            {
                Id = Randomness.Text(),
                ColumnOne = Randomness.Text(),
                ColumnTwo = Randomness.Text()
            };
        }
    }
}