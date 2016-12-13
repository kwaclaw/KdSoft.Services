using KdSoft.Utils;
using System;
using System.Data.Common;
using System.Data.SqlClient;
#if NET451
using System.Transactions;
#endif

namespace KdSoft.Data
{
    /// <summary>
    /// Base class for abstracting a database or data access layer (DAL).
    /// Works together with DbContext{C} to manage connections.
    /// </summary>
    /// <typeparam name="D">For type-safe sub-typing.</typeparam>
    public class Database<D>: IDisposable where D: Database<D>, new()
    {
        /// <summary>
        /// Recommended Maximum number of concurrent database queries on a single MARS connection.
        /// </summary>
        public const int MaxConcurrency = 10;

        #region Static and Factory Members

        static ObjectPool<D> pool;

        //TODO make idle cleanup timeout for Database pool configurable
        static Database() {
            pool = new ObjectPool<D>(TimeSpan.FromSeconds(10), 1000);
        }

        static D InternalOpen(IDbContext dbContext, bool openNew, string name, Action<D> init) {
            if (dbContext == null)
                throw new ArgumentNullException("dbContext");
            var result = pool.Borrow(() => new D());
            result.dbContext = dbContext;
            result.conn = null;
            result.isPooled = true;
            result.openNew = openNew;
            if (name != null)
                result.Name = name;
            if (init != null)
                init(result);
            return result;
        }

        /// <summary>
        /// Initializes new instance from object pool. Will reuse existing connection with the same name.
        /// </summary>
        /// <param name="dbContext">IDbContext instance, must *not* be <c>null</c>.</param>
        /// <param name="name">Name of instance, will replace existing name if not <c>null</c>.</param>
        /// <param name="init">Addition initialization steps. Optional.</param>
        /// <returns>Initialized instance.</returns>
        public static D Open(IDbContext dbContext, string name = null, Action<D> init = null) {
            return InternalOpen(dbContext, false, name, init);
        }

        /// <summary>
        ///     Initializes new instance from object pool. Will use  a new connection for the given name.
        ///     Use this when executling multiple transactions under the same connection name.
        /// </summary>
        /// <param name="dbContext">IDbContext instance, must *not* be <c>null</c>.</param>
        /// <param name="name">Name of instance, will replace existing name if not <c>null</c>.</param>
        /// <param name="init">Addition initialization steps. Optional.</param>
        /// <returns>Initialized instance.</returns>
        public static D OpenNew(IDbContext dbContext, string name = null, Action<D> init = null) {
            return InternalOpen(dbContext, true, name, init);
        }

        #endregion

        /// <summary>
        /// Constructor for use with ObjectPpool. 
        /// </summary>
        protected Database() { }  // for use with ObjectPool

        /// <summary>
        ///     Constructor for explicit use.
        /// </summary>
        /// <param name="dbContext">IDbContext instance, must *not* be <c>null</c>.</param>
        /// <param name="openNew">
        ///     Indicates if a new connection for the given Name should be opened, or if an existing one
        ///     should be re-used. <c>true</c> for new connection, <c>false</c> for existing connection.
        ///     Use <c>true</c> when executling multiple transactions under the same connection name.
        /// </param>
        /// <param name="name">
        ///     Name of instance and name of DB connection to be used.
        ///     The Name property can also be set in constructor body.
        /// </param>
        protected Database(IDbContext dbContext, bool openNew = false, string name = null) {
            if (dbContext == null)
                throw new ArgumentNullException("dbContext");
            this.openNew = openNew;
            this.dbContext = dbContext;
            this.Name = name;
        }

        bool openNew;
        bool isPooled;

        IDbContext dbContext;
        protected IDbContext DbContext { get { return dbContext; } }

        DbConnection conn;
        protected DbConnection Conn {
            get {
                if (conn == null)
                    return conn = openNew ? DbContext.OpenNewConnection(Name) : DbContext.OpenConnection(Name);
                else
                    return conn;
            }
        }
 
        /// <summary>
        /// Gets triggered when the Name changes. Allows reacting to connection changes.
        /// </summary>
        /// <param name="oldName"></param>
        protected virtual void NameChanged(string oldName) { }

        string name;
        public string Name {
            get { return name; }
            protected set {
                if (!string.Equals(this.name, value, StringComparison.OrdinalIgnoreCase)) {
                    var oldName = this.name;
                    this.name = value;
                    NameChanged(oldName);
                }
            }
        }

#if NET451
        public void Enlist(Transaction tx) {
            var sqlConn = (SqlConnection)Conn;
            sqlConn.EnlistTransaction(tx); 
        }
#endif

        //public IDbTransaction BeginTransaction(System.Data.IsolationLevel il) {
        //    return Conn.BeginTransaction(il);
        //}

        //public IDbTransaction BeginTransaction() {
        //    return Conn.BeginTransaction();
        //}

        // NOTE: Leave out the finalizer altogether if this class doesn't 
        // own unmanaged resources itself, but leave the other methods
        // exactly as they are. 
        //~Database() 
        //{
        //    // Finalizer calls Dispose(false)
        //    Dispose(false);
        //}

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (openNew && conn != null) {
                    conn.Close();
                    conn = null;
                }
                // if not created from pool, do not return it to pool
                if (!isPooled)
                    return;
                // free managed resources
                dbContext = null; // don't call dispose on it, it is not under our control
                // return to object pool
                pool.Return((D)this);
            }
            // free native resources if there are any.
            //if (nativeResource != IntPtr.Zero) {
            //  Marshal.FreeHGlobal(nativeResource);
            //  nativeResource = IntPtr.Zero;
            //}
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected class ExposeDatabase : IDatabase
        {
            protected readonly D db;

            public ExposeDatabase(D db) {
                this.db = db;
            }


            public DbConnection Conn {
                get { return db.Conn; }
            }

            public IDbContext DbContext {
                get { return db.DbContext; }
            }
        }
    }
}
