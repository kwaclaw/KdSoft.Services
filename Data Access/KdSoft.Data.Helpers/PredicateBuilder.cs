using Dapper;
using KdSoft.Data.Models.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace KdSoft.Data.Helpers
{
    public static class PredicateBuilder
    {
        // this converts the field value string to an object according to the field's data type;
        // it also modifies the input string value depending on operator, if necessary
        static object CheckFieldValue(string fieldValue, FieldDescriptor fieldDesc, string opKey) {
            if (fieldDesc.DataType == typeof(string)) {
                switch (opKey) {
                    case Operators.StartsWith:
                        fieldValue = fieldValue + '%';
                        break;
                    case Operators.Contains:
                        fieldValue = '%' + fieldValue + '%';
                        break;
                    default:
                        break;
                }
            }

            return fieldDesc.ConvertFieldValueString(fieldValue);
        }

        public static string BuildFilterClause(
            Filter filter,
            FieldDescriptor fieldDescriptor,
            DynamicParameters args,
            int argIndex
        ) {
            string result = "";
            string columnName = fieldDescriptor.DbTable.Alias + "." + fieldDescriptor.DbName;
            string opKey = filter.@Operator.ToUpperInvariant();

            object fieldValue = CheckFieldValue(filter.Value, fieldDescriptor, opKey);

            string paramName = string.Format("@p_{0}_{1}", filter.Field, argIndex);
            args.Add(paramName, fieldValue, fieldDescriptor.DbDataType);
            string operatorPattern = Utils.OperatorMapper[opKey];
            result = columnName + string.Format(operatorPattern, paramName);

            return result;
        }

        // not really needed when using Dapper
        public static void AppendInSetFilterClause(
            InSetFilter filter,
            FieldDescriptor fieldDescriptor,
            DynamicParameters args,
            ref int argIndex,
            StringBuilder sb
        ) {
            int parIndex = argIndex;
            string paramNameTemplate = string.Format("@p_{0}", filter.Field) + "_{0}";
            string columnName = fieldDescriptor.DbTable.Alias + "." + fieldDescriptor.DbName;

            sb.Append(columnName);
            sb.Append(" IN (");

            foreach (var value in filter.Values) {
                var fieldValue = fieldDescriptor.ConvertFieldValueString(value);
                var paramName = string.Format(paramNameTemplate, parIndex++);

                args.Add(paramName, fieldValue, fieldDescriptor.DbDataType);

                sb.Append(paramName);
                sb.Append(',');
            }

            if ((parIndex - argIndex) > 0) {
                argIndex = parIndex;
                sb.Length = sb.Length - 1;
            }
            sb.Append(')');
        }

        /// <summary>
        /// Creates filter criteria based on a given sort order to allow only rows to be
        /// returned that would come next in the order, after a given example record.
        /// </summary>
        /// <param name="sorts">The sort order of the query.</param>
        /// <param name="getFieldStringValue">Delegate that returns stringified field values for the example record.</param>
        /// <returns>Predicate that filters out "prior" rows.</returns>
        public static Predicate CreateLastRecordFilter(List<Sort> sorts, Func<string, string> getFieldStringValue) {
            var orFilters = new List<Predicate>();

            for (int indx = 0; indx < sorts.Count; indx++) {
                var andFilters = new List<Predicate>();

                for (int higherIndx = 0; higherIndx < indx; higherIndx++) {
                    var higherSort = sorts[higherIndx];
                    var higherFilter = new Filter(higherSort.Field, "eq", getFieldStringValue(higherSort.Field));
                    andFilters.Add(higherFilter);
                }

                var thisSort = sorts[indx];
                var sortDir = thisSort.Dir == null ? string.Empty : thisSort.Dir.Trim().ToUpperInvariant();
                var op = sortDir == "DESC" ? "lt" : "gt";
                var thisFilter = new Filter(thisSort.Field, op, getFieldStringValue(thisSort.Field));

                Predicate thisPredicate;
                if (andFilters.Count == 0)
                    thisPredicate = thisFilter;
                else {
                    andFilters.Add(thisFilter);
                    thisPredicate = new AndPredicate(andFilters);
                }

                orFilters.Add(thisPredicate);
            }

            if (orFilters.Count == 0)
                return null;
            else if (orFilters.Count == 1)
                return orFilters[0];
            else
                return new OrPredicate(orFilters);
        }
    }
}
