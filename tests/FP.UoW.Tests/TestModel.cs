namespace FP.UoW.Tests
{
    public class TestModel
    {
        public string Id { get; set; }

        public string ColumnOne { get; set; }

        public string ColumnTwo { get; set; }

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