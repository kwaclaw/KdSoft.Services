using KdSoft.Utils;
using System;
using System.Threading.Tasks;

namespace KdSoft.Data.Helpers
{
    public class TimedCache<K>: IDisposable
    {
        ConcurrentTimedLifeCycleManager<K, CacheEntry<object>> cache;

        public TimedCache(TimeSpan reapPeriod) {
            cache = new ConcurrentTimedLifeCycleManager<K, CacheEntry<object>>(reapPeriod);
        }

        class CacheEntry<T>: TimedLifeCycleAware where T : class
        {
            public readonly T Value;
            public CacheEntry(T value, TimeSpan lifeSpan) : base(lifeSpan) {
                this.Value = value;
            }
        }

        public async Task<T> GetOrAdd<T>(K key, Func<Task<T>> valueFactory, TimeSpan lifeSpan) {
            var newValue = new Lazy<Task<T>>(valueFactory);
            var newEntry = new CacheEntry<object>(newValue, lifeSpan);

            var entry = cache.GetOrAdd(key, newEntry);
            try {
                // need to await the factory task because we need to remove the cache entry if it fails
                var lazy = (Lazy<Task<T>>)entry.Value;
                return await lazy.Value;
            }
            catch {
                cache.TryTerminate(key);
                throw;
            }
        }

        public T GetOrAddSync<T>(K key, Func<T> valueFactory, TimeSpan lifeSpan) {
            var newValue = new Lazy<T>(valueFactory);
            var newEntry = new CacheEntry<object>(newValue, lifeSpan);

            var entry = cache.GetOrAdd(key, newEntry);
            try {
                // need to remove the cache entry if it fails
                var lazy = (Lazy<T>)entry.Value;
                return lazy.Value;
            }
            catch {
                cache.TryTerminate(key);
                throw;
            }
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                cache.Dispose();
            }
            // free native resources if there are any.
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
