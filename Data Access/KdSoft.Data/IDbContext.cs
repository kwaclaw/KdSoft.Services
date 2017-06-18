using System;
using System.Data.Common;
using System.Globalization;

namespace KdSoft.Data
{
    /// <summary>
    /// Interface for managing connection strings and connections. Helps keep connections alive 
    /// between database and service calls, mainly to prevent a transaction from implicitly
    /// being elevated to a Distributed Transaction when not necessary.
    /// </summary>
    /// <remarks>Database connections opened from this instance will only be closed/returned to the pool
    /// when this instance is closed/disposed of.</remarks>
    public interface IDbContext: IDisposable
    {
        /// <summary>
        /// Retrieves connection setting by name. Connection settings are intended to be shared
        /// between all instances of the same implementation class.
        /// </summary>
        /// <param name="name">Name of connection setting.</param>
        DbConnectionSetting GetConnectionSetting(string name);

        /// <summary>
        /// Opens a connection for the specified connection setting. Will re-use connections
        /// already opened for the same name.
        /// </summary>
        /// <param name="name">Name of <see cref="DbConnectionSetting"/> to use.</param>
        /// <returns>New or existing connection.</returns>
        DbConnection OpenConnection(string name);

        /// <summary>
        /// Opens a new connection for the specified connection setting - 'new' can mean a connection
        /// available from the connection pool. Will not re-use connections already opened for the same name.
        /// </summary>
        /// <param name="name">Name of <see cref="DbConnectionSetting"/> to use.</param>
        /// <returns>New connection.</returns>
        /// <remarks>Since a database connection can only have one transaction context during its life-time
        /// (that is, until it is closed/returned to the pool), it will be necessary to call OpenNewConnection(string)
        /// instead of <see cref="OpenConnection(string)"/> if we want to use a connection with the same name
        /// from the same <see cref="IDbContext"/> instance inside a new transaction.<br/>
        /// In other words, we cannot have multiple transactions on the same connection before it is closed,
        /// which means, before this <see cref="IDbContext"/> instance is closed.</remarks>
        DbConnection OpenNewConnection(string name);

        /// <summary>
        /// Culture to use when database access should include culture context.
        /// </summary>
        CultureInfo Culture { get; }
    }
}
