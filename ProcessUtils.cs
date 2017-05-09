using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace KdSoft.Utils
{
    public static class ProcessUtils
    {
        class CancelRegHolder
        {
            public CancellationTokenRegistration Value;
        }

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
                throw new ArgumentNullException("Must handle redirected error output.", nameof(errorReceived));
            if (startInfo.RedirectStandardOutput && outputReceived == null)
                throw new ArgumentNullException("Must handle redirected standard output.", nameof(outputReceived));

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
