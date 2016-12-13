using System.Data;
using System.Data.Common;
using System.Globalization;

namespace KdSoft.Data
{
    public interface IDbContext
    {
        DbConnection OpenConnection(string name);

        DbConnection OpenNewConnection(string name);

        CultureInfo Culture { get; }
    }
}
