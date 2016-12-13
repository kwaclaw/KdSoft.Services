
namespace KdSoft.Data.Models.Shared
{
    public class Sort
    {
        public Sort() { }

        public Sort(string field, string dir = null) {
            this.Field = field;
            this.Dir = dir;
        }

        public string Field { get; set; }
        public string Dir { get; set; }

        public static Sort Asc(string field) {
            return new Sort(field, "ASC");
        }

        public static Sort Desc(string field) {
            return new Sort(field, "DESC");
        }
    }
}
