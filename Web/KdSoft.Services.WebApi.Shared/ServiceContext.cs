using KdSoft.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace QLine.Services.WebApi
{
    public class ServiceContext: IServiceContext
    {
        ILoggerFactory loggerFactory;
        IHttpContextAccessor accessor;

        public ServiceContext(ILoggerFactory loggerFactory, IHttpContextAccessor accessor) {
            this.loggerFactory = loggerFactory;
            this.accessor = accessor;
        }

        public ILoggerFactory LoggerFactory {
            get {
                return loggerFactory;
            }
        }

        public CultureInfo Culture {
            get {
                var requestCultureFeature = (IRequestCultureFeature)accessor.HttpContext?.Features[typeof(IRequestCultureFeature)];
                if (requestCultureFeature != null)
                    return requestCultureFeature.RequestCulture.UICulture;
                else
                    return CultureInfo.CurrentUICulture;
            }
        }
    }
}
