using System;
using System.Text;

namespace FP.UoW.Tests
{
    public static class Randomness
    {
        private const int MIN_STRING_LENGTH = 8;
        private const int MAX_STRING_LENGTH = 32;

        private const string ALPHABET = "ABCDEFGHILMNOPQRSTUVWXYZabcdefghilmnopqrstuvwxyz01234567890";

        private static readonly Random RNG = new Random(0xFEDE);

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
    }
}