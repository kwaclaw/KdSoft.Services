
namespace KdSoft.Services.Protobuf
{
    public static class BaseConverters
    {
        public static Date ToProtoDate(this System.DateTime date) {
            return new Date { Day = date.Day, Month = date.Month, Year = date.Year };
        }

        public static Time ToProtoTime(this System.DateTime time) {
            return new Time { MilliSecond = time.Millisecond, Second = time.Second, Minute = time.Minute, Hour = time.Hour };
        }

        public static Time ToProtoTime(this System.TimeSpan time) {
            return new Time { MilliSecond = time.Milliseconds, Second = time.Seconds, Minute = time.Minutes, Hour = time.Hours };
        }

        public static DateTime ToProtoDateTime(this System.DateTime dt) {
            return new DateTime { Date = dt.ToProtoDate(), Time = dt.ToProtoTime() };
        }

        public static System.TimeSpan ToTimeSpan(this Time pt) {
            return new System.TimeSpan(0, pt.Hour, pt.Minute, pt.Second, pt.MilliSecond);
        }

        public static System.DateTime ToDateTime(this Date pd) {
            return new System.DateTime(pd.Year, pd.Month, pd.Day);
        }

        public static System.DateTime ToDateTime(this DateTime pdt) {
            return new System.DateTime(pdt.Date.Year, pdt.Date.Month, pdt.Date.Day, pdt.Time.Hour, pdt.Time.Minute, pdt.Time.Second, pdt.Time.MilliSecond);
        }
    }
}
