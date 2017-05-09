﻿using KdSoft.Models;
using KdSoft.Utils;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KdSoft.Services
{
    public abstract class JobQueue<T, K, S>
        where T : IIdentifiable<K>
        where K : IEquatable<K>
        where S : IIdentifiable<K>
    {
        const int maxConcurrentJobs = 10;
        // const int dequeueThreshold = maxConcurrentJobs * 2 / 3;
        const int shutDownActive = 999;

        int isShutDown;
        Task runJobsTask;

        readonly object statusSyncObj = new object();
        public enum QueueStatus: int
        {
            Stopped = 0,
            Pending = 999,
            Active = 555
        }
        QueueStatus queueStatus = QueueStatus.Stopped;

        protected readonly object jobSyncObj = new object();

        protected JobQueue() { }

        #region Must Override

        protected abstract Task Initialize();

        /// <summary>
        /// Map of currently running jobs.
        /// </summary>
        protected abstract IDictionary<T, Task> RunningJobs { get; }

        /// <summary>
        /// Runs new job instance based on job argument. Must not throw an exception!
        /// Implements any applicable logic concerning conflicts/concurrency with running jobs.
        /// </summary>
        /// <param name="job">Specifies job to run.</param>
        /// <returns><see cref="Task"/> instance representing job.</returns>
        protected abstract Task RunJob(T job);

        /// <summary>
        /// Returns queued and finished jobs as an atomic operation. Must not throw an exception!
        /// </summary>
        /// <param name="waitingIds">Job ids to be included in the finished jobs result.</param>
        /// <param name="maxConcurrentJobs">Maximum number of concurrently runnable queued jobs to return.</param>
        /// <returns>Queued and finished jobs.</returns>
        protected abstract Task<(IEnumerable<T> queued, IEnumerable<S> finished)> GetWaitingJobs(
            IEnumerable<K> waitingIds, int maxConcurrentJobs
        );

        /// <summary>
        /// Adds a new job specification to the queue.
        /// </summary>
        /// <param name="job">Specifies job to queue.</param>
        /// <returns><see cref="Task"/> representing queuing operation.</returns>
        protected abstract Task EnqueueJob(T job);

        #endregion

        Task CreateAndStartJob(T job) {
            // this would implement any logic concerning conflicts/concurrency with running jobs
            var jobTask = RunJob(job);  // must not throw!
            if (jobTask == null)
                return null;

            var finishedTask = jobTask.ContinueWith((jt) => {
                lock (jobSyncObj) {
                    RunningJobs.Remove(job);
                }
                return jt;
            });

            var result = finishedTask.Unwrap();
            RunningJobs.Add(job, result);

            return result;
        }

        // starts queued jobs as long as they do not have the same job name as a currently running job 
        async Task<IList<Task>> DequeueAndRun() {
            var newJobTasks = new List<Task>();

            var waitingIds = GetWaitingJobIds();
            var (queuedJobs, finishedJobs) = await GetWaitingJobs(waitingIds, maxConcurrentJobs).ConfigureAwait(false);
            ProcessFinishedJobs(finishedJobs);

            foreach (var queuedJob in queuedJobs) {
                Task newJobTask = null;
                int jobCount = 0;
                lock (jobSyncObj) {
                    // this must not throw!
                    newJobTask = CreateAndStartJob(queuedJob);
                    jobCount = RunningJobs.Count;
                }
                if (newJobTask != null) {
                    newJobTasks.Add(newJobTask);
                    if (jobCount >= maxConcurrentJobs)
                        break;
                }
            }

            return newJobTasks;
        }

        /// <summary>
        /// Dequeues and runs jobs repeatedly until no more queued jobs are left.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// To keep the queue running it needs to be re-started whenever a new job comes in
        /// while the queue is empty, but while it is running a new job being queued should
        /// not trigger a re-start. This are the state changes:
        /// A) Before a DequeueAndRun() gets called, the queueStatus is set to "QueueStatus.Active"
        /// - 1) When the DequeueAndRun() returns with new jobs started:
        ///      the loop waits for these jobs to finish and continues (to call another DequeueAndRun())
        /// - 2) When the DequeueAndRun() returns without new jobs started, the value of queueStatus is checked:
        ///      - a) queueStatus == "QueueStatus.Active": stop loop and change to "QueueStatus.Stopped"
        ///      - b) queueStatus != "QueueStatus.Active": continue loop because new jobs were queued in the meantime
        /// B) When QueueJob() gets called, then after the queuing was successful the value of queueStatus is checked:
        ///      - a) queueStatus == "QueueStatus.Stopped": change to "QueueStatus.Pending" and call RunJobs()
        ///      - b) queueStatus != "QueueStatus.Stopped": change to "QueueStatus.Pending" anyways
        /// C) When Start() gets called, isShutDown is set to 0 and queueStatus is checked: (do same as for B)
        ///      - a) queueStatus == "QueueStatus.Stopped": change to "QueueStatus.Pending" and call RunJobs()
        ///      - b) queueStatus != "QueueStatus.Stopped": change to "QueueStatus.Pending"
        /// </remarks>
        async Task RunJobs() {
            //TODO Disable constraints
            //var disableTasks = new List<Task<int>>(tableLoaderMap.Count);
            //foreach (var tableLoader in tableLoaderMap.Values) {
            //    var disableTask = tableLoader.EnableConstraints(db, false);
            //    if (disableTask != null)
            //        disableTasks.Add(disableTask);
            //}
            //await Task.WhenAll(disableTasks).ConfigureAwait(false);

            while (isShutDown == 0) {
                lock (statusSyncObj) {
                    queueStatus = QueueStatus.Active;
                }
                // this must not throw!
                var newJobTasks = await DequeueAndRun().ConfigureAwait(false);
                if (newJobTasks.Count == 0) {
                    bool stopLoop = false;
                    // we only stop the loop if no new jobs were queued since DequeueAndRun() was called
                    lock (statusSyncObj) {
                        if (queueStatus == QueueStatus.Active) {
                            stopLoop = true;
                            queueStatus = QueueStatus.Stopped;
                        }
                    }
                    if (stopLoop)
                        break;
                    else
                        continue;  // skip the waiting step, no tasks to wait for
                }

                // continue when at least one job is done - if we wait for all then we could get held up
                var firstTaskDone = await Task.WhenAny(newJobTasks).ConfigureAwait(false);
                firstTaskDone.Forget();
            };
            //TODO re-enable constraints - might give errors
            // re-enable constraints - one at a time (serialized, not concurrently - no MARS limit to consider);
            // the reason is that re-enabling them requires table locks and we could run into
            // lock conflicts as they all have FK constraints referencing the load.LOAD_LOG table
            //foreach (var tableLoader in tableLoaderMap.Values) {
            //    try {
            //        var enableTask = tableLoader.EnableConstraints(db, true);
            //        if (enableTask != null)
            //            await enableTask.ConfigureAwait(false);
            //    }
            //    catch (AggregateException aggEx) {
            //        errorMessages.AppendLine(aggEx.CombineMessages());
            //    }
            //    catch (Exception ex) {
            //        errorMessages.AppendLine(ex.Message);
            //    }
            //}

        }

        bool CheckRestart() {
            bool doRestart = false;
            lock (statusSyncObj) {
                doRestart = queueStatus == QueueStatus.Stopped;
                queueStatus = QueueStatus.Pending;
            }

            if (doRestart) {
                runJobsTask = RunJobs();
            }
            return doRestart;
        }

        public async Task<bool> Start() {
            this.isShutDown = 0;
            await Initialize().ConfigureAwait(false);
            return CheckRestart();
        }

        public async Task Shutdown(bool wait) {
            int lastShutDown = Interlocked.CompareExchange(ref isShutDown, shutDownActive, 0);
            if (lastShutDown != 0)
                throw new InvalidOperationException("Shutdown already in progress.");

            if (wait) {
                // repeatedly wait for the runningJobs tasks until no more jobs are available
                ICollection<Task> rjs;
                while (true && isShutDown != 0) {
                    lock (jobSyncObj) {
                        rjs = RunningJobs.Values;
                    }
                    Task rj = rjs.Count == 0 ? null : Task.WhenAll(rjs);
                    if (rj == null)
                        return;
                    await rj.ConfigureAwait(false);
                }
            }
        }

        public async Task QueueJob(T job) {
            if (isShutDown != 0)
                throw new InvalidOperationException("Job queue is shut down");
            await EnqueueJob(job).ConfigureAwait(false);
            // re-start the queue if necessary
#pragma warning disable S1481 // Unused local variables should be removed
            var restartTask = CheckRestart();
#pragma warning restore S1481 // Unused local variables should be removed
        }

        #region Job Completion

        readonly object completionSync = new object();
        readonly LinkedList<JobCompletion> jobCompletions = new LinkedList<JobCompletion>();

        class JobCompletion
        {
            readonly HashSet<K> jobIds;
            readonly List<S> completedJobs;
            public readonly TaskCompletionSource<IList<S>> CompletionSource;

            public JobCompletion(IEnumerable<K> jobIds) {
                this.jobIds = new HashSet<K>(jobIds);
                completedJobs = new List<S>();
                CompletionSource = new TaskCompletionSource<IList<S>>();
            }

            public bool Finished(IEnumerable<S> finishedJobs) {
                foreach (var job in finishedJobs) {
                    if (jobIds.Remove(job.Id)) {
                        completedJobs.Add(job);
                    }
                }
                bool result = jobIds.Count == 0;
                if (result)
                    CompletionSource.TrySetResult(completedJobs);
                return result;
            }

            public void UnionInto(HashSet<K> globalSet) {
                globalSet.UnionWith(jobIds);
            }
        }

        HashSet<K> GetWaitingJobIds() {
            var result = new HashSet<K>();
            lock (completionSync) {
                var completionNode = jobCompletions.First;
                while (completionNode != null) {
                    var jobCompletion = completionNode.Value;
                    jobCompletion.UnionInto(result);
                    completionNode = completionNode.Next;
                }
            }
            return result;
        }

        void ProcessFinishedJobs(IEnumerable<S> finishedJobs) {
            lock (completionSync) {
                var completionNode = jobCompletions.First;
                while (completionNode != null) {
                    var jobCompletion = completionNode.Value;
                    if (jobCompletion.Finished(finishedJobs)) {
                        // we handle this as Task continuation so that we can include Cancellations
                        // jobCompletions.Remove(completionNode);
                    }
                    completionNode = completionNode.Next;
                }
            }
        }

        public Task<IList<S>> JobsFinished(IEnumerable<K> jobIds, CancellationToken cancelToken) {
            var completion = new JobCompletion(jobIds);
            LinkedListNode<JobCompletion> completionNode;
            lock (completionSync) {
                completionNode = jobCompletions.AddLast(completion);
            }

            var completionTask = completion.CompletionSource.Task;
            completionTask.ContinueWith(ct => {
                lock (completionSync) {
                    jobCompletions.Remove(completionNode);
                }
            }, TaskContinuationOptions.ExecuteSynchronously);

            cancelToken.Register(() => completion.CompletionSource.TrySetCanceled());
            return completionTask;
        }

        #endregion
    }
}
