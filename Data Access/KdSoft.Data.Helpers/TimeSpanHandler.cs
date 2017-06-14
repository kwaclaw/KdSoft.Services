using Dapper;
using System;
using System.Data;
using System.Data.SqlClient;

namespace KdSoft.Data.Helpers
{
    /// <summary>
    /// Dapper TypeHandler when TimeSpan is stored as SQL type BigInt (int64)
    /// </summary>
    public class TimeSpanHandler: SqlMapper.TypeHandler<TimeSpan>
    {
        public override TimeSpan Parse(object value) {
            return new TimeSpan((Int64)value);
        }

        // Currently there is a Dapper bug where SetValue never gets called for built-in types
        public override void SetValue(IDbDataParameter parameter, TimeSpan value) {
            parameter.Value = value == null ? (object)DBNull.Value : (object)value.Ticks;
            if (parameter is SqlParameter) {
                ((SqlParameter)parameter).SqlDbType = SqlDbType.BigInt;
            }
        }
    }
}
