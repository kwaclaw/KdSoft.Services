using KdSoft.Utils;
using SqlModeller.Model;
using System;
using System.Collections.Generic;
using System.Data;

namespace KdSoft.Data.Helpers
{
    public class FieldDescriptor
    {
        public readonly Table DbTable;
        public readonly string DbName;
        public readonly Type DataType;
        public readonly DbType? DbDataType;
        public readonly bool IsSelectable = true;
        public readonly bool IsFilterable = true;
        public readonly bool IsSortable = true;
        public readonly bool IsAggregate = false;

        Func<string, object> convertFieldValue;

        FieldDescriptor() { }

        public FieldDescriptor(
            Table dbTable,
            string dbName,
            Type dataType,
            DbType? dbType = null,
            bool isSelectable = true,
            bool isFilterable = true,
            bool isSortable = true,
            bool isAggregate = false
        ) {
            this.DbTable = dbTable;
            this.DbName = dbName;
            this.DataType = dataType;
            this.DbDataType = dbType;
            this.IsFilterable = isFilterable;
            this.IsSortable = isSortable;
            this.IsSelectable = isSelectable;
            this.IsAggregate = isAggregate;

            if (dataType == typeof(string)) {
                convertFieldValue = fieldValue => fieldValue;
            }
            else if (dataType == typeof(DateTime)) {
                convertFieldValue = fieldValue => DateTime.Parse(fieldValue);
            }
            else if (dataType == typeof(DateTimeOffset)) {
                convertFieldValue = fieldValue => DateTimeOffset.Parse(fieldValue);
            }
            else if (dataType == typeof(TimeSpan)) {
                // TimeSpan strings are assumed to be in ISO8601 format
                convertFieldValue = fieldValue => TimeSpanExtensions.ParseIso(fieldValue).Ticks; //TODO remove workaround once Dapper can handle TimeSpan
            }
            else {
                convertFieldValue = fieldValue => Convert.ChangeType(fieldValue, dataType);
            }
        }

        public object ConvertFieldValueString(string fieldValue) {
            return convertFieldValue(fieldValue);
        }

        public IEnumerable<object> ConvertFieldValueStrings(IEnumerable<string> fieldValues) {
                foreach (var fieldValue in fieldValues)
                    yield return convertFieldValue(fieldValue);
        }

        public static string ToString(object value, IFormatProvider provider = null) {
            if (value == null)
                throw new ArgumentNullException("value");
            var objType = value.GetType();
            if (value is DateTime)
                return ((DateTime)value).ToString("o", provider);
            else if (value is DateTimeOffset)
                return ((DateTimeOffset)value).ToString("o", provider);
            else if (value is TimeSpan)
                return ((TimeSpan)value).ToIsoString();
            else
                return value.ToString();
        }
    }
}
