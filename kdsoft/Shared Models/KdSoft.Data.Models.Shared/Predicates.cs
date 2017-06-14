using System.Collections.Generic;

namespace KdSoft.Data.Models.Shared
{
    public class Predicate: Visitable
    {
        public static NotPredicate Not(Predicate operand) {
            return new NotPredicate(operand);
        }

        public static AndPredicate And(params Predicate[] operands) {
            return new AndPredicate(operands);
        }

        public static AndPredicate And(IEnumerable<Predicate> operands) {
            return new AndPredicate(operands);
        }

        public static OrPredicate Or(params Predicate[] operands) {
            return new OrPredicate(operands);
        }

        public static OrPredicate Or(IEnumerable<Predicate> operands) {
            return new OrPredicate(operands);
        }
    }

    public class TruePredicate: Predicate
    {
    }

    public class FalsePredicate: Predicate
    {
    }

    public class NotPredicate: Predicate
    {
        public NotPredicate() { }

        public NotPredicate(Predicate operand) {
            this.Operand = operand;
        }

        public Predicate Operand { get; set; }
    }

    public class AndPredicate: Predicate
    {
        public AndPredicate() { }

        public AndPredicate(IEnumerable<Predicate> operands) {
            this.Operands = operands;
        }

        // can cause ambiguous constructor errors
        public AndPredicate(params Predicate[] operands) {
            this.Operands = operands;
        }

        public IEnumerable<Predicate> Operands { get; set; }
    }

    public class OrPredicate: Predicate
    {
        public OrPredicate() { }

        public OrPredicate(IEnumerable<Predicate> operands) {
            this.Operands = operands;
        }

        // can cause ambiguous constructor errors
        public OrPredicate(params Predicate[] operands) {
            this.Operands = operands;
        }

        public IEnumerable<Predicate> Operands { get; set; }
    }
}
