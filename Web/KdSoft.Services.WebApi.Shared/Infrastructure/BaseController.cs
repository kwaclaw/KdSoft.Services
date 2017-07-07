using Microsoft.AspNetCore.Mvc;
using System;
using System.Security;

namespace KdSoft.Services.WebApi.Infrastructure
{
    /// <summary>
    /// Basic controller implementation to derive from.
    /// </summary>
    public class BaseController: Controller {
        protected IServiceProvider ServiceProvider { get; private set; }

        // gets instantiated once per request
        protected BaseController(IServiceProvider serviceProvider) {
            this.ServiceProvider = serviceProvider;
        }

        protected void CheckMaxRecordCount(int count) {
            if (count > BaseWebApiConfig.MaxRecordCount)
                throw new SecurityException("Maximum request count exceeded.");
        }
    }
}