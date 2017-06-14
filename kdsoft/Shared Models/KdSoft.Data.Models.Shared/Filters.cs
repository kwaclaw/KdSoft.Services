using System.Collections.Generic;

namespace KdSoft.Data.Models.Shared
{
    public class Filter: Predicate
    {
        public Filter() {}

        public Filter(string field, string @operator, string value) {
            this.Field = field;
            this.@Operator = @operator;
            this.Value = value;
        }

        public string Field { get; set; }
        public string @Operator { get; set; }
        public string Value { get; set; }

        public static Filter Equal(string field, string value) {
            return new Filter(field, Operators.Equal, value);
        }

        public static Filter NotEqual(string field, string value) {
            return new Filter(field, Operators.NotEqual, value);
        }

        public static Filter GreaterThan(string field, string value) {
            return new Filter(field, Operators.GreaterThan, value);
        }

        public static Filter LessThan(string field, string value) {
            return new Filter(field, Operators.LessThan, value);
        }

        public static Filter GreaterEqual(string field, string value) {
            return new Filter(field, Operators.GreaterEqual, value);
        }

        public static Filter LessEqual(string field, string value) {
            return new Filter(field, Operators.LessEqual, value);
        }

        public static Filter Contains(string field, string value) {
            return new Filter(field, Operators.Contains, value);
        }

        public static Filter StartsWith(string field, string value) {
            return new Filter(field, Operators.StartsWith, value);
        }

        public static IsNullFilter IsNull(string field) {
            return new IsNullFilter(field);
        }

        public static IsNotNullFilter IsNotNull(string field) {
            return new IsNotNullFilter(field);
        }

        public static InSetFilter InSet(string field, IEnumerable<string> values) {
            return new InSetFilter(field, values);
        }
    }

    public class IsNullFilter: Predicate
    {
        public IsNullFilter() { }

        public IsNullFilter(string field) {
            this.Field = field;
        }

        public string Field { get; set; }
    }

    public class IsNotNullFilter: Predicate
    {
        public IsNotNullFilter() { }

        public IsNotNullFilter(string field) {
            this.Field = field;
        }

        public string Field { get; set; }
    }

    public abstract class SetFilter: Predicate
    {
        public string Field { get; set; }
    }

    public class InSetFilter: SetFilter
    {
        public InSetFilter() { }

        public InSetFilter(string field, IEnumerable<string> values) {
            this.Field = field;
            this.Values = values;
        }

        public IEnumerable<string> Values { get; set; }
    }
}
