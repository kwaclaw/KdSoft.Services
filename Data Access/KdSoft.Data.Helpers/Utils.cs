using Dapper;
using KdSoft.Data.Models.Shared;
using KdSoft.Reflection;
using SqlModeller.Model;
using SqlModeller.Model.Order;
using System;
using System.Collections.Generic;

namespace KdSoft.Data.Helpers
{
    public static class Utils
    {
        // static constructors are called exactly once
        static Utils() {
            OperatorMapper = new Dictionary<string, string>();
            InitOperatorMapper(OperatorMapper);
        }

        public static bool IsEqualToSecond(double dbTime1, double dbTime2) {
            var cvTime1 = (int)Math.Round(dbTime1 * 10000.0);
            var cvTime2 = (int)Math.Round(dbTime2 * 10000.0);
            return cvTime1 == cvTime2;
        }

        public static readonly TimeSpan TwentyFourHours = TimeSpan.FromHours(24);

        public static int GetStringsHashCode(IEnumerable<string> strings) {
            int result = 0;
            foreach (var str in strings)
                result = result ^ (str == null ? 0 : str.GetHashCode());
            return result;
        }

        #region Dynamic SQL Filtering

        public static readonly Dictionary<string, string> OperatorMapper;

        static void InitOperatorMapper(Dictionary<string, string> operatorMapper) {
            string eqPatNum = " = {0}";
            string neqPatNum = " <> {0}";
            string gtPatNum = " > {0}";
            string ltPatNum = " < {0}";
            string gteqPatNum = " >= {0}";
            string lteqPatNum = " <= {0}";

            operatorMapper.Add(Operators.StartsWith, " LIKE {0}");
            operatorMapper.Add(Operators.Contains, " LIKE {0}");

            // default
            operatorMapper.Add(Operators.Equal, eqPatNum);
            operatorMapper.Add(Operators.NotEqual, neqPatNum);
            operatorMapper.Add(Operators.GreaterThan, gtPatNum);
            operatorMapper.Add(Operators.LessThan, ltPatNum);
            operatorMapper.Add(Operators.GreaterEqual, gteqPatNum);
            operatorMapper.Add(Operators.LessEqual, lteqPatNum);
        }

        public static DynamicParameters ConvertDictionaryToArgList(IDictionary<string, object> argList) {
            var args = new DynamicParameters();
            foreach (var kvp in argList) {
                args.Add(kvp.Key, kvp.Value);
            }
            return args;
        }

        public static void ApplyUniqueSortKey(this SortFilter sf, string field, string direction) {
            var sorts = sf.Sorts ?? new List<Sort>();
            if (null == sorts.Find(s => string.Compare(s.Field, field, StringComparison.OrdinalIgnoreCase) == 0))
                sorts.Add(new Sort(field, direction));
            sf.Sorts = sorts;
        }

        public static void ApplyUniqueSortKeys(this SortFilter sf, IEnumerable<Sort> uniqueSortKeys) {
            var sorts = sf.Sorts ?? new List<Sort>();
            foreach (var usk in uniqueSortKeys)
                if (null == sorts.Find(s => string.Compare(s.Field, usk.Field, StringComparison.OrdinalIgnoreCase) == 0))
                    sorts.Add(usk);
            sf.Sorts = sorts;
        }

        // Any changes to the Sorts (see ApplyUniqueSortKey) must have been applied already
        public static void ApplyLastRecordPredicate(this SortNextFilter snf, Dictionary<string, FieldDescriptor> fieldMap) {
            var lastRec = snf.LastRecord as IDictionary<string, object>;
            if (lastRec == null) {
                lastRec = new Dictionary<string, object>();
                var propValues = PropertyUtils.GetPropertyValues(snf.LastRecord);
                foreach (var propValue in propValues) {
                    lastRec[propValue.Key] = propValue.Value;
                }
            }

            Func<string, string> getFieldStringValue = (fieldName) => FieldDescriptor.ToString(lastRec[fieldName]);

            var lastRecFilter = PredicateBuilder.CreateLastRecordFilter(snf.Sorts, getFieldStringValue);
            var fullFilter = snf.Filter != null ? new AndPredicate(lastRecFilter, snf.Filter) : lastRecFilter;
            snf.Filter = fullFilter;
        }

        #endregion Dynamic SQL Filtering

        //public static string GetUniqueParameter(DynamicParameters args, string baseName) {
        //    var lookup = (SqlMapper.IParameterLookup)args;
        //    var sb = new StringBuilder(baseName.Length + 6);
        //    sb.Append("@p_");
        //    sb.Append(baseName);
        //    sb.Append("_0");
        //    var numIndx = Convert.ToInt32('0');
        //    for (int indx = 1; indx < 10; indx++) {
        //        string result = sb.ToString();
        //        if (lookup[result] == null)
        //            return result;
        //        sb[sb.Length - 1] = Convert.ToChar(numIndx + indx);
        //    }
        //    throw new InvalidOperationException("Could not find a unique parameter name.");
        //}


        public static string GetCheckedSortClause(this Sort sort, Dictionary<string, FieldDescriptor> sortFields) {
            var fm = sortFields[sort.Field];
            if (!fm.IsSortable)
                throw new InvalidOperationException(string.Format("Field '{0}' is not sortable.", sort.Field));
            string aliasPrefix = string.IsNullOrWhiteSpace(fm.DbTable.Alias) ? string.Empty : fm.DbTable.Alias + '.';
            string result = aliasPrefix + fm.DbName;
            if (string.IsNullOrWhiteSpace(sort.Dir))
                return result;
            string sd = sort.Dir.Trim().ToUpperInvariant();
            switch (sd) {
                case "ASC":
                case "DESC":
                    return result + " " + sd;
                default:
                    throw new InvalidOperationException(string.Format("Invalid sort direction '{0}'.", sort.Dir));
            }
        }

        public static OrderByColumn GetCheckedSortColumn(this Sort sort, Dictionary<string, FieldDescriptor> sortFields) {
            var fm = sortFields[sort.Field];
            if (!fm.IsSortable)
                throw new InvalidOperationException(string.Format("Field '{0}' is not sortable.", sort.Field));

            var direction = OrderDir.Asc;
            if (!string.IsNullOrWhiteSpace(sort.Dir)) {
                string sd = sort.Dir.Trim().ToUpperInvariant();
                switch (sd) {
                    case "ASC":
                        break;
                    case "DESC":
                        direction = OrderDir.Desc;
                        break;
                    default:
                        throw new InvalidOperationException(string.Format("Invalid sort direction '{0}'.", sort.Dir));
                }
            }

            return new OrderByColumn(fm.DbTable.Alias, fm.DbName, direction);
        }
    }
}

