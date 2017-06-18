using KdSoft.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;

namespace KdSoft.Data
{
    /// <summary>
    /// Helps managing connection strings and connections. Helps keep connections alive 
    /// between database and service calls, mainly to prevent a transaction from implicitly
    /// being elevated to a Distributed Transaction when not necessary.
    /// </summary>
    /// <typeparam name="C">For type-safe sub-typing.</typeparam>
    /// <remarks>Database connections opened from this instance will only be closed/returned to the pool
    /// when this instance is closed/disposed of.</remarks>
    public class DbContext<C>: IDisposable, IDbContext where C : DbContext<C>, new()
    {
        #region Static and Factory Members

        static readonly Dictionary<string, DbConnectionSetting> connectionSettings;
        static readonly ObjectPool<C> pool;

        //TODO make idle cleanup timeout for DbContext pool configurable
        static DbContext() {
            connectionSettings = new Dictionary<string, DbConnectionSetting>(StringComparer.OrdinalIgnoreCase);
            pool = new ObjectPool<C>(TimeSpan.FromSeconds(10), 1000);
        }

        public static void Initialize(IEnumerable<DbConnectionSetting> connSettings = null) {
            if (connSettings != null) {
                lock (connectionSettings) {
                    foreach (var cs in connSettings)
                        connectionSettings.Add(cs.Name, cs);
                }
            }
        }

        public static void AddConnectionSetting(DbConnectionSetting connSetting) {
            lock (connectionSettings) {
                connectionSettings.Add(connSetting.Name, connSetting);
            }
        }

        public static void SetConnectionSetting(DbConnectionSetting connSetting) {
            lock (connectionSettings) {
                connectionSettings[connSetting.Name] = connSetting;
            }
        }

        public static void RemoveConnectionSetting(string name) {
            lock (connectionSettings) {
                connectionSettings.Remove(name);
            }
        }

        public static C Create(CultureInfo culture = null) {
            var result = pool.Borrow(() => new C());
            result.culture = culture;
            return result;
        }

        #endregion

        readonly ConcurrentDictionary<string, DbConnection> connections;

        CultureInfo IDbContext.Culture => throw new NotImplementedException();

        protected DbContext() {
            connections = new ConcurrentDictionary<string, DbConnection>(StringComparer.OrdinalIgnoreCase);
        }

        DbConnection InternalOpenConnection(string name) {
            DbConnectionSetting connSetting;
            lock (connectionSettings) {
                connSetting = connectionSettings[name];
            }
            string connStr = connSetting.ConnectionString;
            var result = connSetting.ProviderFactory.CreateConnection();
            try {
                result.ConnectionString = connStr;
                if (result.State != ConnectionState.Open)
                    result.Open();
                return result;
            }
            catch {
                result.Dispose();
                throw;
            }
        }


        #region IDbContext

        DbConnectionSetting IDbContext.GetConnectionSetting(string name) {
            return GetConnectionSetting(name);
        }

        /// <inheritdoc cref="IDbContext.OpenConnection(string)"/>
        public DbConnection OpenConnection(string name) {
            return connections.GetOrAdd(name, this.InternalOpenConnection);
        }

        /// <inheritdoc cref="IDbContext.OpenNewConnection(string)"/>
        public DbConnection OpenNewConnection(string name) {
            return InternalOpenConnection(name);
        }

        CultureInfo culture;
        /// <inheritdoc cref="IDbContext.Culture"/>
        public CultureInfo Culture {
            get { return culture ?? CultureInfo.CurrentUICulture; }
        }

        #endregion

        public static bool TryGetConnectionSetting(string name, out DbConnectionSetting value) {
            lock (connectionSettings) {
                return connectionSettings.TryGetValue(name, out value);
            }
        }

        /// <inheritdoc cref="IDbContext.GetConnectionSetting(string)"/>
        public static DbConnectionSetting GetConnectionSetting(string name) {
            lock (connectionSettings) {
                return connectionSettings[name];
            }
        }

        // NOTE: Leave out the finalizer altogether if this class doesn't 
        // own unmanaged resources itself, but leave the other methods
        // exactly as they are. 
        //~DbContext() 
        //{
        //    // Finalizer calls Dispose(false)
        //    Dispose(false);
        //}

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                // free managed resources
                try {
                    foreach (var conn in connections) {
                        try {
                            conn.Value.Dispose();  // may throw an exception!! (this is unexpected)
                        }
                        catch { }
                    }
                }
                catch { }
                finally {
                    connections.Clear();
                    // return to object pool
                    pool.Return((C)this);
                }
            }
            // free native resources if there are any.
            //if (nativeResource != IntPtr.Zero) {
            //  Marshal.FreeHGlobal(nativeResource);
            //  nativeResource = IntPtr.Zero;
            //}
        }

        /// <summary>
        /// Closes connections.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
