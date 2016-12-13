using KdSoft.Data.Models.Shared;
using SqlModeller.Model;
using SqlModeller.Model.Select;
using SqlModeller.Model.Where;
using SqlModeller.Shorthand;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KdSoft.Data.Helpers
{
    public static class Extensions
    {
        public static string ColumnFullName(string tableAlias, string fieldName) {
            if (string.IsNullOrWhiteSpace(tableAlias)) {
                return fieldName;
            }
            return string.Format("{0}.{1}", tableAlias, fieldName);
        }

        public static string ColumnFullName(this Table table, string fieldName) {
            return ColumnFullName(table.Alias, fieldName);
        }

        public static SelectQuery Select(this SelectQuery query, Table table, string field) {
            query.SelectColumns.Add(new SqlColumnSelector(ColumnFullName(table.Alias, field)));
            return query;
        }

        public static SelectQuery WhereIsNull(this SelectQuery query, string tableAlias, string field) {
            string sql;
            if (string.IsNullOrWhiteSpace(tableAlias))
                sql = string.Format("{0} IS NULL", field);
            else
                sql = string.Format("{0}.{1} IS NULL", tableAlias, field);
            return query.Where(sql);
        }

        public static SelectQuery WhereIsNull(this SelectQuery query, Table table, string field) {
            return WhereIsNull(query, table.Alias, field);
        }

        public static WhereFilterCollection WhereIsNull(this WhereFilterCollection query, string tableAlias, string field) {
            string sql;
            if (string.IsNullOrWhiteSpace(tableAlias))
                sql = string.Format("{0} IS NULL", field);
            else
                sql = string.Format("{0}.{1} IS NULL", tableAlias, field);
            return query.Where(sql);
        }

        public static WhereFilterCollection WhereIsNull(this WhereFilterCollection query, Table table, string field) {
            return WhereIsNull(query, table.Alias, field);
        }

        public static SelectQuery ApplySortFilter(
            this SelectQuery query,
            SortNextFilter sortFilter,
            IEnumerable<Sort> uniqueSortKeys,
            Func<Dictionary<string, FieldDescriptor>, SqlPredicateVisitor> getPredicateVisitor,
            Dictionary<string, FieldDescriptor> fieldMap
        ) {
            sortFilter.ApplyUniqueSortKeys(uniqueSortKeys);
            if (sortFilter.LastRecord != null)
                sortFilter.ApplyLastRecordPredicate(fieldMap);

            string filterStr = string.Empty;
            if (sortFilter.Filter != null) {
                var sqlPredVisitor = getPredicateVisitor(fieldMap);
                sortFilter.Filter.Accept(sqlPredVisitor);
                filterStr = sqlPredVisitor.Sql;
            }

            if (!string.IsNullOrEmpty(filterStr))
                query.Where(Combine.And).Where(filterStr);

            var sorts = sortFilter.Sorts;
            if (sorts != null && sorts.Any()) {
                foreach (var sort in sorts) {
                    query.OrderByColumns.Add(sort.GetCheckedSortColumn(fieldMap));
                }
            }

            return query;
        }

    }
}
