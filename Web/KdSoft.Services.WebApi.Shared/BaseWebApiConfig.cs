using System;
using System.Globalization;

namespace QLine.Services.WebApi
{
    public class BaseWebApiConfig
    {
        public static TimeSpan PermissionsRefreshTime { get; set; }

        public static int MaxRecordCount { get; set; }

        public static CultureInfo DefaultCulture { get; set; }
    }
}
