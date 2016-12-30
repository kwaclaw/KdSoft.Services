using System;
using System.Text;

namespace KdSoft.Data.Helpers
{
    public static class BuilderUtils
    {
        public const string NullVal = "NULL";

        /// <summary>
        /// Appends a string quoted with single quotes - useful for SQL generation.
        /// Single quotes within the string are SQL-escaped with two single quotes.
        /// If the string is null, a non-quoted string NULL will be appended instead.
        /// </summary>
        public static StringBuilder AppendQuoted(this StringBuilder sb, string str) {
            if (str == null)
                sb.Append(NullVal);
            else {
                sb.Append('\'').Append(str.Replace("'", @"''")).Append('\'');
            }
            return sb;
        }

        public static Action<StringBuilder> AsQuoted(string val) {
            return (sb) => sb.AppendQuoted(val);
        }

        /// <summary>
        /// Appends a stringified nullable value type - useful for SQL generation.
        /// If the value is null, a non-quoted string NULL will be appended instead.
        /// </summary>
        public static StringBuilder AppendPlain<T>(this StringBuilder sb, T? val) where T : struct {
            if (val == null)
                sb.Append(NullVal);
            else {
                sb.Append(val.Value.ToString());
            }
            return sb;
        }

        public static Action<StringBuilder> AsPlain<T>(T? val) where T : struct {
            return (sb) => sb.AppendPlain(val);
        }

        /// <summary>
        /// Appends a stringified nullable value type quoted with single quotes - useful for SQL generation.
        /// Single quotes within the stringified value are SQL-escaped with two single quotes.
        /// If the value is null, a non-quoted string NULL will be appended instead.
        /// </summary>
        public static StringBuilder AppendQuoted<T>(this StringBuilder sb, T? val) where T : struct {
            if (val == null)
                sb.Append(NullVal);
            else {
                sb.Append('\'').Append(val.Value.ToString().Replace("'", @"''")).Append('\'');
            }
            return sb;
        }

        public static Action<StringBuilder> AsQuoted<T>(T? val) where T : struct {
            return (sb) => sb.AppendQuoted(val);
        }

        /// <summary>
        /// Appends a stringified object (struct or reference type) - useful for SQL generation.
        /// If the value is null, a non-quoted string NULL will be appended instead.
        /// </summary>
        public static StringBuilder AppendPlain<T>(this StringBuilder sb, T val) {
            if (val == null)
                sb.Append(NullVal);
            else {
                sb.Append(val.ToString());
            }
            return sb;
        }

        public static Action<StringBuilder> AsPlain<T>(T val) {
            return (sb) => sb.AppendPlain(val);
        }


        /// <summary>
        /// Appends a stringified object quoted with single quotes - useful for SQL generation.
        /// Single quotes within the stringified value are SQL-escaped with two single quotes.
        /// If the value is null, a non-quoted string NULL will be appended instead.
        /// </summary>
        public static StringBuilder AppendQuoted<T>(this StringBuilder sb, T val) {
            if (val == null)
                sb.Append(NullVal);
            else {
                sb.Append('\'').Append(val.ToString().Replace("'", @"''")).Append('\'');
            }
            return sb;
        }

        public static Action<StringBuilder> AsQuoted<T>(T val) {
            return (sb) => sb.AppendQuoted(val);
        }

        // Prerequisite: appendValues must *not* be empty
        public static void AppendSqlValuesRow(StringBuilder sb, params Action<StringBuilder>[] appendValues) {
            sb.Append('(');
            foreach (var append in appendValues) {
                append(sb);
                sb.Append(',');
            }
            sb.Length = sb.Length - 1;  // remove last comma
            sb.AppendLine("),");
        }

        public static StringBuilder BuildSqlWithValuesClause(string sql1, string sql2, Func<StringBuilder, int> appendValueRows) {
            var sb = new StringBuilder();
            sb.Append(sql1);
            int rowCount = appendValueRows(sb);
            if (rowCount == 0)
                return null;
            // remove comma at end of last line
            int indx = sb.Length - 1;
            while (sb[indx] != ',')
                indx--;
            sb.Length = indx;
            sb.Append(sql2);
            return sb;
        }
    }
}

