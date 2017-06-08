using System.Diagnostics;

namespace KdSoft.Utils
{
    /// <summary>
    /// Holds exit code and standard output/errors returned by a <see cref="Process"/>.
    /// </summary>
    public class ProcessResult
    {
        public int ExitCode { get; set; }
        public string Errors { get; set; }
        public string Output { get; set; }
    }
}
