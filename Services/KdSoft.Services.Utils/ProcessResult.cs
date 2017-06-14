using System.Diagnostics;

namespace KdSoft.Utils
{
    /// <summary>
    /// Holds exit code and standard output/errors returned by a <see cref="Process"/>.
    /// </summary>
    public class ProcessResult
    {
        /// <summary />
        public int ExitCode { get; set; }
        /// <summary />
        public string Errors { get; set; }
        /// <summary />
        public string Output { get; set; }
    }
}
