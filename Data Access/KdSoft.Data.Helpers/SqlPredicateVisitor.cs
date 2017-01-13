using Dapper;
using KdSoft.Data.Models.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace KdSoft.Data.Helpers
{
    public class SqlPredicateVisitor  //: IVisitor<Filter>, IVisitor<NotPredicate>, IVisitor<AndPredicate>, IVisitor<OrPredicate>
    {
        protected readonly StringBuilder sb;
        protected readonly IReadOnlyDictionary<string, FieldDescriptor> fieldMap;
        protected readonly DynamicParameters args;
        protected int argStartIndex;

        public SqlPredicateVisitor(IReadOnlyDictionary<string, FieldDescriptor> fieldMap, DynamicParameters args, int argStartIndex = 0) {
            sb = new StringBuilder();
            this.fieldMap = fieldMap;
            this.args = args;
            this.argStartIndex = argStartIndex;
        }

        public void Visit(Filter element) {
            FieldDescriptor fieldDesc;
            if (!this.fieldMap.TryGetValue(element.Field, out fieldDesc))
                throw new ArgumentException(string.Format("Field '{0}' is not a valid filterable field.", element.Field));

            sb.Append("( ");
            sb.Append(PredicateBuilder.BuildFilterClause(element, fieldDesc, args, argStartIndex++));
            sb.Append(" )");
        }

        // We could implement "Not" by using a Filter with the "opposite" operator
        public void Visit(NotPredicate element) {
            sb.Append(" ( NOT ");
            element.Operand.Accept(this);
            sb.Append(" )");
        }

        public void Visit(IsNullFilter element) {
            FieldDescriptor fieldDesc;
            if (!this.fieldMap.TryGetValue(element.Field, out fieldDesc))
                throw new ArgumentException(string.Format("Field '{0}' is not a valid filterable field.", element.Field));

            sb.Append(" ( ");
            sb.Append(fieldDesc.DbTable.Alias).Append(".").Append(fieldDesc.DbName);
            sb.Append(" IS NULL )");
        }

        public void Visit(IsNotNullFilter element) {
            FieldDescriptor fieldDesc;
            if (!this.fieldMap.TryGetValue(element.Field, out fieldDesc))
                throw new ArgumentException(string.Format("Field '{0}' is not a valid filterable field.", element.Field));

            sb.Append(" ( ");
            sb.Append(fieldDesc.DbTable.Alias).Append(".").Append(fieldDesc.DbName);
            sb.Append(" IS NOT NULL )");
        }

        public void Visit(AndPredicate element) {
            bool isFirst = true;
            sb.Append("( ");
            foreach (var op in element.Operands) {
                if (isFirst)
                    isFirst = false;
                else
                    sb.Append(" AND ");
                op.Accept(this);
            }
            sb.Append(" )");
        }

        public void Visit(OrPredicate element) {
            bool isFirst = true;
            sb.Append("( ");
            foreach (var op in element.Operands) {
                if (isFirst)
                    isFirst = false;
                else
                    sb.Append(" OR ");
                op.Accept(this);
            }
            sb.Append(" )");
        }

        public void Visit(InSetFilter element) {
            FieldDescriptor fieldDesc;
            if (!this.fieldMap.TryGetValue(element.Field, out fieldDesc))
                throw new ArgumentException(string.Format("Field '{0}' is not a valid filterable field.", element.Field));

            sb.Append("( ");

            // we could use this if we did not target Dapper:
            // PredicateBuilder.AppendInSetFilterClause(element, fieldDesc, args, ref argStartIndex, sb);

            string paramName = string.Format("@p_{0}_{1}", element.Field, argStartIndex++);
            string columnName = fieldDesc.DbTable.Alias + "." + fieldDesc.DbName;

            sb.Append(columnName);
            sb.Append(" IN ");
            sb.Append(paramName);
            sb.Append(" )");

            var fieldValues = fieldDesc.ConvertFieldValueStrings(element.Values);
            args.Add(paramName, fieldValues);  // do not pass a DbType here, Dapper handles IEnumerables differently
        }

        public string Sql {
            get { return sb.ToString(); }
        }

        public virtual void Reset() {
            sb.Clear();
            argStartIndex = 0;
        }
    }

}
