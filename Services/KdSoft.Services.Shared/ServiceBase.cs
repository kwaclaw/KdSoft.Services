using KdSoft.Data;
using KdSoft.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Transactions;

namespace KdSoft.Services
{
    public class ServiceBase<S, T, C>: IDisposable
        where S : ServiceBase<S, T, C>, new()
        where T : IServiceContext
        where C : DbContext<C>, new()
    {
        #region Static and Factory Members

        static ObjectPool<S> pool;

        //TODO make idle cleanup timeout for Service pool configurable
        static ServiceBase() {
            pool = new ObjectPool<S>(TimeSpan.FromSeconds(10), 1000);
        }

        public static S Create(T callContext, C dbContext = null) {
            // no lock needed, no one else has access to this object yet
            var result = pool.Borrow(() => new S());
            result.Initialize(callContext, dbContext);
            return result;
        }

        #endregion

        bool disposeContext;

        T callContext;
        protected T CallContext {
            get { return callContext; }
            set { callContext = value; }  // use with care - mainly intended for testing (mock services0
        }

        C dbContext;
        protected C DbContext {
            get { return dbContext; }
        }

        ILogger logger;
        protected ILogger Logger {
            get {
                if (logger == null)
                    logger = callContext.LoggerFactory.CreateLogger(this.GetType().FullName);
                return logger;
            }
        }

        protected string LangIsoCode {
            get { return CallContext.Culture.ThreeLetterISOLanguageName; }
        }

        protected ServiceBase() { }

        // not thread-safe
        void Initialize(T callContext, C dbContext = null) {
            this.callContext = callContext;
            // dispose context only if no context was passed in and we have to create one
            disposeContext = dbContext == null;
            this.dbContext = disposeContext ? DbContext<C>.Create(CallContext.Culture) : dbContext;
        }

        // NOTE: Leave out the finalizer altogether if this class doesn't 
        // own unmanaged resources itself, but leave the other methods
        // exactly as they are. 
        //~ServiceBase() 
        //{
        //    // Finalizer calls Dispose(false)
        //    Dispose(false);
        //}

        protected virtual D OpenDb<D>(string name = null, Action<D> init = null) where D : Database<D>, new() {
            return Database<D>.Open(DbContext, name, init);
        }

        /// <summary>
        /// Open a NEW connection from the pool without re-using an already open connection from the Context.
        /// This is necessary when using multiple TransactionScopes (e.g. in a loop, in the same Service call)
        /// with the same Context, since a DB connection that is already open will not be enlisted in the new TransactionScope.
        /// </summary>
        /// <typeparam name="D"></typeparam>
        /// <param name="name"></param>
        /// <param name="init"></param>
        /// <returns></returns>
        protected virtual D OpenNewDb<D>(string name = null, Action<D> init = null) where D : Database<D>, new() {
            return Database<D>.OpenNew(DbContext, name, init);
        }

        protected static TransactionScope CreateAsyncTxScope(IsolationLevel isolation = IsolationLevel.ReadCommitted) {
            var txOptions = new TransactionOptions { IsolationLevel = isolation };
            return new TransactionScope(TransactionScopeOption.Required, txOptions, TransactionScopeAsyncFlowOption.Enabled);
        }

        protected static TransactionScope CreateAsyncTxScope(TransactionScopeOption scopeOption, IsolationLevel isolation = IsolationLevel.ReadCommitted) {
            var txOptions = new TransactionOptions { IsolationLevel = isolation };
            return new TransactionScope(scopeOption, txOptions, TransactionScopeAsyncFlowOption.Enabled);
        }

        protected static TransactionScope CreateAsyncTxScope(TransactionScopeOption scopeOption, TimeSpan scopeTimeout, IsolationLevel isolation = IsolationLevel.ReadCommitted) {
            var txOptions = new TransactionOptions { IsolationLevel = isolation, Timeout = scopeTimeout };
            return new TransactionScope(scopeOption, txOptions, TransactionScopeAsyncFlowOption.Enabled);
        }

        protected static TransactionScope CreateAsyncTxScope(Transaction transactionToUse, TimeSpan timeout) {
            return new TransactionScope(transactionToUse, timeout, TransactionScopeAsyncFlowOption.Enabled);
        }

        // if the Context gets disposed then connections get closed
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                // free managed resources
                var ctx = dbContext;
                if (ctx != null) {
                    dbContext = null;
                    if (disposeContext)
                        ctx.Dispose();
                }
                callContext = default(T);
                // return to object pool
                pool.Return((S)this);
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
    }
}
