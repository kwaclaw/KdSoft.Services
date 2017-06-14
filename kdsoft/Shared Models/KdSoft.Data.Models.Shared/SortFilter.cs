using System.Collections.Generic;

namespace KdSoft.Data.Models.Shared
{
    public class SortFilter
    {
        public Predicate Filter { get; set; }
        public List<Sort> Sorts { get; set; }
    }

    public class SortNextFilter<T>: SortFilter
    {
        public T LastRecord { get; set; }

        public SortNextFilter CloneAsSortNextFilter() {
            return (SortNextFilter)MemberwiseClone();
        }
    }

    public class SortNextFilter: SortNextFilter<object>
    {
        // we can use SortNextFilter<T> to force deserialization for T, and then clone
        // as SortNextFilter for type compatibility in further processing
        public static SortNextFilter CloneFrom<T>(SortNextFilter<T> source) {
            return new Shared.SortNextFilter
            {
                Filter = source.Filter,
                Sorts = source.Sorts,
                LastRecord = source.LastRecord
            };
        }
    }
}
