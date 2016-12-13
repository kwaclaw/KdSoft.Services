using System;
using System.Transactions;

namespace KdSoft.Services
{
    public static class Utils
    {
        public static readonly string[] EmptyStrings = new string[0];

        // syntactic sugar for chaining statements
        public static T Complete<T>(this TransactionScope txScope, T value) {
            txScope.Complete();
            return value;
        }

        /// <summary>
        /// Splits the string and removes any leading/trailing whitespace from each result item.
        /// </summary>
        /// <param name="original">The input string.</param>
        /// <param name="splitChars">The characters to use as separators.</param>
        /// <returns>An array of strings parsed from the input <paramref name="original"/> string.</returns>
        public static string[] SplitString(string original, params char[] splitChars) {
            if (String.IsNullOrEmpty(original))
                return EmptyStrings;

            var result = original.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
            int resIndx = 0;
            for (int indx = 0; indx < result.Length; indx++) {
                string resPart = result[indx].Trim();
                if (resPart.Length == 0)
                    continue;
                result[resIndx++] = resPart;
            }
            Array.Resize<string>(ref result, resIndx);
            return result;
        }

        public static DateTimeOffset ToPrecision(this DateTimeOffset dto, TimestampPrecision precision) {
            long remainder;
            switch (precision) {
                case TimestampPrecision.Day:
                    remainder = dto.Ticks % TimeSpan.TicksPerDay;
                    break;
                case TimestampPrecision.Hour:
                    remainder = dto.Ticks % TimeSpan.TicksPerHour;
                    break;
                case TimestampPrecision.Second:
                    remainder = dto.Ticks % TimeSpan.TicksPerSecond;
                    break;
                case TimestampPrecision.Millisecond:
                    remainder = dto.Ticks % TimeSpan.TicksPerMillisecond;
                    break;
                default:
                    throw new ArgumentException("Invalid precision.", "precision");
            }
            return new DateTimeOffset(dto.Ticks - remainder, dto.Offset);
        }
    }

    public enum TimestampPrecision
    {
        Day, Hour, Minute, Second, Millisecond
    }
}
