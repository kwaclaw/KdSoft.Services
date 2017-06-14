using System;
using System.Data;
using System.Data.Common;
using System.Globalization;

namespace KdSoft.Data
{
    public interface IDbContext: IDisposable
    {
        DbConnection OpenConnection(string name);

        DbConnection OpenNewConnection(string name);

        CultureInfo Culture { get; }
    }
}
