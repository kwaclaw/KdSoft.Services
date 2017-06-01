using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace KdSoft.Utils
{
    /// <summary>
    /// Process utility routines.
    /// </summary>
    public static class ProcessUtils
    {
        class CancelRegHolder
        {
            public CancellationTokenRegistration Value;
        }

        /// <summary>
        /// Runs process as specified by a <see cref="ProcessStartInfo"/> instance.
        /// </summary>
        /// <param name="startInfo">Specifies process to run.</param>
        /// <param name="cancelToken">Cancellation token to stop process.</param>
        /// <param name="log"><see cref="ILogger"/> instance to use for logging. If <c>null</c> no logging is performed.</param>
        /// <param name="errorReceived">Callback to handle process standard error output. Turns on standard error redirection.</param>
        /// <param name="outputReceived">Callback to handle process standard output. Turns on standard output redirection.</param>
        /// <returns>Task representing process.</returns>
        public static Task<Process> Run(
            ProcessStartInfo startInfo,
            CancellationToken cancelToken,
            ILogger log,
            DataReceivedEventHandler errorReceived = null,
            DataReceivedEventHandler outputReceived = null
        ) {
            if (startInfo == null)
                throw new ArgumentNullException(nameof(startInfo));
            if (startInfo.RedirectStandardError && errorReceived == null)
                throw new ArgumentNullException(nameof(errorReceived), "Must handle redirected error output.");
            if (startInfo.RedirectStandardOutput && outputReceived == null)
                throw new ArgumentNullException(nameof(outputReceived), "Must handle redirected standard output.");

            var tcs = new TaskCompletionSource<Process>();
            var process = new Process {
                StartInfo = startInfo,
                EnableRaisingEvents = true,
            };

            EventHandler prcExited = (s, e) => {
                log?.LogDebug("Exiting process {0}.", process.Id);
                tcs.TrySetResult(process);
            };

            Action<object> cancelHandler = (obj) => {
                try {
                    log?.LogDebug("Killing process {0}.", process.Id);
                    process.Kill();
                }
                catch (InvalidOperationException) {
                    //
                }
                try {
                    var holder = (CancelRegHolder)obj;
                    holder.Value.Dispose();
                }
                catch {
                    //
                }
            };


            process.Exited += prcExited;
            if (errorReceived != null) {
                process.StartInfo.RedirectStandardError = true;
                process.ErrorDataReceived += errorReceived;
            }
            if (outputReceived != null) {
                process.StartInfo.RedirectStandardOutput = true;
                process.OutputDataReceived += outputReceived;
            }

            if (cancelToken != CancellationToken.None) {
                var holder = new CancelRegHolder();
                var reg = cancelToken.Register(cancelHandler, holder);
                holder.Value = reg;
            }
            process.Start();
            log?.LogDebug("Started process {0}.", process.Id);

            if (errorReceived != null)
                process.BeginErrorReadLine();
            if (outputReceived != null)
                process.BeginOutputReadLine();

            return tcs.Task;
        }
    }
}
