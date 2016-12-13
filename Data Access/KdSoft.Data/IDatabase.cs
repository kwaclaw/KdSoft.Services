using System;
using System.Data.Common;

namespace KdSoft.Data
{
    public interface IDatabase
    {
        DbConnection Conn { get; }
        IDbContext DbContext { get; }
    }
}
