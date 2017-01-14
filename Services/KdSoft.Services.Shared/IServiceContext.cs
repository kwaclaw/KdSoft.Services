using Microsoft.Extensions.Logging;
using System;
using System.Globalization;

namespace KdSoft.Services
{
    public interface IServiceContext
    {
        IServiceProvider Provider { get; }
        CultureInfo Culture { get; }
    }
}
