using System;
using System.Text;

namespace FP.UoW.Sql.Tests.Infrastructure
{
    public static class Randomness
    {
        private const int MIN_STRING_LENGTH = 8;
        private const int MAX_STRING_LENGTH = 32;

        private const int DATABASE_NAME_LENGTH = 16;

        private const int MIN_NUMBER = 100_000;
        private const int MAX_NUMBER = 999_999;

        private const string ALPHABET = "ABCDEFGHILMNOPQRSTUVWXYZabcdefghilmnopqrstuvwxyz01234567890";

        private static readonly Random RNG = new Random();

        public static string Text()
        {
            var length = RNG.Next(MIN_STRING_LENGTH, MAX_STRING_LENGTH);

            var sb = new StringBuilder(length);

            for (var i = 0; i < length; i++)
            {
                var rngIndex = RNG.Next(0, ALPHABET.Length);

                sb.Append(ALPHABET[rngIndex]);
            }

            return sb.ToString();
        }

        public static string DatabaseName()
        {
            var guid = Guid.NewGuid();

            var guidString = $"_{guid:N}";

            return guidString;
        }

        public static int Number()
        {
            return RNG.Next(MIN_NUMBER, MAX_NUMBER);
        }
    }
}