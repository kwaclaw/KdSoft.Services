using KdSoft.Services.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Concurrent;
using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KdSoft.Services.WebApi.Infrastructure
{
    public class BaseController: Controller {
        protected IServiceProvider ServiceProvider { get; private set; }
        protected IAuthorizationScope AuthScope { get; private set; }

        static readonly ConcurrentDictionary<Type, IAuthorizationScope> authorizationScopeMap = new ConcurrentDictionary<Type, IAuthorizationScope>();

        IAuthorizationScope GetAuthorizationScope(IServiceProvider serviceProvider) {
            var result = authorizationScopeMap.GetOrAdd(this.GetType(), memberType => {
                AuthorizationScopeAttribute asAttribute = null;
                var atts = (AuthorizationScopeAttribute[])Attribute.GetCustomAttributes(memberType, typeof(AuthorizationScopeAttribute), true);
                if (atts != null && atts.Length > 0)
                    asAttribute = atts[0];
                if (asAttribute == null)
                    throw new InvalidOperationException("Missing AuthorizationScopeAttribute.");
                return (IAuthorizationScope)serviceProvider.GetService(asAttribute.Type);
            });
            return result;
        }

        // gets instantiated once per request
        protected BaseController(IServiceProvider serviceProvider) {
            this.ServiceProvider = serviceProvider;
            this.AuthScope = GetAuthorizationScope(serviceProvider);
        }

        async Task AddAuthorizationClaims() {
            int? userKey = User.GetUserKeyIfExists();
            string userName = User.GetUserName();
            string authType = User.GetUserAuthType();
            var claimsResult = await AuthScope.ClaimsCache.GetClaimPropertyValuesAsync(userName, authType, userKey).ConfigureAwait(false);

            var claims = claimsResult.Claims;
            if (claims != null && claims.Count > 0) {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                claimsIdentity.AddClaims(claims);
            }
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