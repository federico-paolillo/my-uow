﻿using System;
using System.Text;

namespace FP.UoW.Examples.ConsoleApplication
{
    public static class Randomness
    {
        private const int MIN_STRING_LENGTH = 8;
        private const int MAX_STRING_LENGTH = 32;

        private const int MIN_NUMBER = 100_000;
        private const int MAX_NUMBER = 999_999;

        private const string ALPHABET = "ABCDEFGHILMNOPQRSTUVWXYZabcdefghilmnopqrstuvwxyz01234567890";

        private static readonly Random rng = new Random(0xFEDE);

        public static string Text()
        {
            var length = rng.Next(MIN_STRING_LENGTH, MAX_STRING_LENGTH);

            var sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                var rngIndex = rng.Next(0, ALPHABET.Length);

                sb.Append(ALPHABET[rngIndex]);
            }

            return sb.ToString();
        }

        public static int Number()
        {
            return rng.Next(MIN_NUMBER, MAX_NUMBER);
        }
    }
}