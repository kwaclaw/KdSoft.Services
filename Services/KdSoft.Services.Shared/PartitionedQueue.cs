using Nito.Collections;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace KdSoft.Services
{
    public class PartitionedQueue<TKey, TData> where TData: class
    {
        ConcurrentDictionary<TKey, Deque<TData>> partitions;

        public PartitionedQueue() {
            partitions = new ConcurrentDictionary<TKey, Deque<TData>>();
        }

        public PartitionedQueue(IEnumerable<KeyValuePair<TKey, TData>> data): this() {
            LoadItems(data);
        }

        public PartitionedQueue(IEnumerable<KeyValuePair<TKey, IEnumerable<TData>>> data): this() {
            LoadQueues(data);
        }

        /// <summary>
        /// Loads data collection of key-value pairs without locking.
        /// Intended for initial data load.
        /// </summary>
        /// <remarks>Items are enqueued in the order they are encountered.</remarks>
        /// <param name="items">Data to load.</param>
        /// <returns>Number of items loaded.</returns>
        protected int LoadItems(IEnumerable<KeyValuePair<TKey, TData>> items) {
            int result = 0;
            foreach (var pair in items) {
                var queue = partitions.GetOrAdd(pair.Key, k => new Deque<TData>());
                queue.AddToBack(pair.Value);
                result++;
            }
            return result;
        }

        /// <summary>
        /// Loads data collection of key-value pairs without locking.
        /// Intended for initial data load.
        /// </summary>
        /// <param name="data">Data to load.</param>
        /// <returns>Number of queues loaded.</returns>
        protected int LoadQueues(IEnumerable<KeyValuePair<TKey, IEnumerable<TData>>> queues) {
            int result = 0;
            foreach (var pair in queues) {
                bool queueCreated = false;
                var queue = partitions.GetOrAdd(pair.Key, k => {
                    queueCreated = true;
                    return new Deque<TData>(pair.Value);
                });
                result++;
                if (queueCreated)
                    continue;
                queue.InsertRange(queue.Count, pair.Value);
            }
            return result;
        }

        public void EnqueueRange(TKey key, IEnumerable<TData> range, Func<TData, bool> trimPredicate = null) {
            var queue = partitions.GetOrAdd(key, k => new Deque<TData>());
            lock (queue) {
                if (trimPredicate != null)
                    TrimQueue(queue, trimPredicate);
                queue.InsertRange(queue.Count, range);
            }
        }

        public void Enqueue(TKey key, TData item, Func<TData, bool> trimPredicate = null) {
            var queue = partitions.GetOrAdd(key, k => new Deque<TData>());
            lock (queue) {
                if (trimPredicate != null)
                    TrimQueue(queue, trimPredicate);
                queue.AddToBack(item);
            }
        }

        /// <summary>
        /// Dequeues items from the queue as long as the predicate returns true.
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="trimPredicate"></param>
        void TrimQueue(Deque<TData> queue, Func<TData, bool> trimPredicate) {
            for (int indx = 0; indx < queue.Count; indx++) {
                var item = queue[indx];
                if (trimPredicate(item))
                    queue.RemoveFromFront();
                else
                    break;
            }
        }

        /// <summary>
        /// Dequeues items from the queue as long as the predicate returns true.
        /// Removes queue if empty.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="trimPredicate"></param>
        /// <returns><c>true</c> when queue found, <c>false</c> otherwise.</returns>
        public bool TrimQueue(TKey key, Func<TData, bool> trimPredicate) {
            Deque<TData> queue;
            if (!partitions.TryGetValue(key, out queue))
                return false;
            lock (queue) {
                Deque<TData> removedQueue;
                TrimQueue(queue, trimPredicate);
                if (queue.Count == 0) {
                    partitions.TryRemove(key, out removedQueue);
                }
            }
            return true;
        }

        /// <summary>
        /// Gives readonly access to the queue using a delegate. if a trimPredicate is supplied,
        /// then it combines trim and access operations as one atomic unit. Trim is performed first.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="access"></param>
        /// <param name="trimPredicate"></param>
        /// <returns><c>true</c> when queue found, <c>false</c> otherwise.</returns>
        public bool GetQueue(TKey key, Action<IReadOnlyList<TData>> access, Func<TData, bool> trimPredicate = null) {
            Deque<TData> queue;
            if (!partitions.TryGetValue(key, out queue))
                return false;
            lock (queue) {
                if (trimPredicate != null) {
                    Deque<TData> removedQueue;
                    TrimQueue(queue, trimPredicate);
                    if (queue.Count == 0) {
                        partitions.TryRemove(key, out removedQueue);
                    }
                }
                access(queue);
            }
            return true;
        }

        public void Clear() {
            partitions.Clear();
        }

        /// <summary>
        /// Trims queues and removes from dictionary if empty.
        /// </summary>
        /// <param name="dequeuePredicate"></param>
        public void TrimQueues(Func<TData, bool> dequeuePredicate) {
            Deque<TData> removedQueue;
            foreach (var pair in partitions) {
                var queue = pair.Value;
                if (queue == null) {  // should not happen
                    partitions.TryRemove(pair.Key, out removedQueue);
                    continue;
                }
                lock (queue) {
                    TrimQueue(queue, dequeuePredicate);
                    if (queue.Count == 0) {
                        partitions.TryRemove(pair.Key, out removedQueue);
                    }
                }
            }
        }
    }
}
