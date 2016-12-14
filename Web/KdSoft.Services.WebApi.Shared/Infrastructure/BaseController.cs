using KdSoft.Services.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KdSoft.Services.WebApi.Infrastructure
{
    public class BaseController: Controller {
        protected IServiceProvider ServiceProvider { get; private set; }
        protected IAuthorizationScope AuthScope { get; private set; }

        static AuthorizationScopeAttribute authScopeAttribute;

        // gets instantiated once per request
        protected BaseController(IServiceProvider serviceProvider) {
            this.ServiceProvider = serviceProvider;
            if (authScopeAttribute == null) {
                var asAttributes = (AuthorizationScopeAttribute[])this.GetType().GetCustomAttributes(typeof(AuthorizationScopeAttribute), true);
                if (asAttributes != null || asAttributes.Length > 0)
                    authScopeAttribute = asAttributes[0];
            }
            if (authScopeAttribute == null)
                throw new InvalidOperationException("Missing AuthorizationScopeAttribute.");
            this.AuthScope = (IAuthorizationScope)serviceProvider.GetService(authScopeAttribute.Type);
        }

        async Task AddAuthorizationClaims() {
            int? userKey = User.GetUserKeyIfExists();
            if (userKey == null)
                return;
            var claimsResult = await AuthScope.ClaimsCache.GetClaimPropertyValuesAsync(User.GetUserKey()).ConfigureAwait(false);
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            claimsIdentity.AddClaims(claimsResult.Claims);
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
            await AddAuthorizationClaims();
            await base.OnActionExecutionAsync(context, next);
        }

        protected void CheckMaxRecordCount(int count) {
            if (count > BaseWebApiConfig.MaxRecordCount)
                throw new SecurityException("Maximum request count exceeded.");
        }
    }
}