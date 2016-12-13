using Microsoft.Extensions.Logging;
using System.Globalization;

namespace KdSoft.Services
{
    public interface IServiceContext
    {
        ILoggerFactory LoggerFactory { get; }
        CultureInfo Culture { get; }
    }
}
