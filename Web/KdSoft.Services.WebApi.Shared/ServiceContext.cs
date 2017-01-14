using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System;
using System.Globalization;

namespace KdSoft.Services.WebApi
{
    public class ServiceContext: IServiceContext
    {
        public ServiceContext(IServiceProvider provider, IHttpContextAccessor accessor) {
            this.Provider = provider;
            this.Accessor = accessor;
        }

        public IServiceProvider Provider { get;}

        public IHttpContextAccessor Accessor { get; }

        public CultureInfo Culture {
            get {
                var requestCultureFeature = (IRequestCultureFeature)Accessor.HttpContext?.Features[typeof(IRequestCultureFeature)];
                if (requestCultureFeature != null)
                    return requestCultureFeature.RequestCulture.UICulture;
                else
                    return CultureInfo.CurrentUICulture;
            }
        }
    }
}
