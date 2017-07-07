using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KdSoft.Services.Security.AspNet
{
    /// <summary>
    /// This filter adds authorization related claims to the <see cref="HttpContext.User"/>.
    /// Different claims will be added depending on the <see cref="IAuthorizationScope"/> instance
    /// associated with the <see cref="Controller"/> type of the request.
    /// This allows scope-specific claims based authorization later in the request pipeline.
    /// </summary>
    public class AuthorizationScopeFilter: IAsyncAuthorizationFilter, IOrderedFilter
    {
        readonly ConcurrentDictionary<Type, IAuthorizationScope> authorizationScopeMap;

        public AuthorizationScopeFilter() {
            authorizationScopeMap = new ConcurrentDictionary<Type, IAuthorizationScope>();
        }

        /// <summary>
        /// Order of filter execution. This filter needs to run first, so the value is negative.
        /// </summary>
        public int Order => -99;

        IAuthorizationScope GetAuthorizationScope(Type controllerType, IServiceProvider provider) {
            var result = authorizationScopeMap.GetOrAdd(controllerType, memberType => {
                AuthorizationScopeAttribute asAttribute = null;
                var atts = (AuthorizationScopeAttribute[])Attribute.GetCustomAttributes(memberType, typeof(AuthorizationScopeAttribute), true);
                if (atts != null && atts.Length > 0)
                    asAttribute = atts[0];
                if (asAttribute == null)
                    return null;
                return (IAuthorizationScope)provider.GetService(asAttribute.Type);
            });
            return result;
        }

        async Task AddAuthorizationClaims(ClaimsPrincipal user, IAuthorizationScope authScope, IServiceProvider provider) {
            try {
                int? userKey = user.GetUserKeyIfExists();
                string userName = user.GetUserName();
                string authType = user.GetUserAuthType();
                var claimsResult = await authScope.ClaimsCache.GetClaimPropertyValuesAsync(userName, authType, userKey).ConfigureAwait(false);

                var claims = claimsResult.Claims;
                if (claims != null && claims.Count > 0) {
                    var claimsIdentity = (ClaimsIdentity)user.Identity;
                    claimsIdentity.AddClaims(claims);
                }
            }
            catch (Exception ex) {
                var logger = (ILogger<AuthorizationScopeFilter>)provider.GetService(typeof(ILogger<AuthorizationScopeFilter>));
                logger?.LogError(0, ex, "Failed to add authorization claims.");
            }
        }

        /// <inheritdoc />
        public Task OnAuthorizationAsync(AuthorizationFilterContext context) {
            var controllerDescripter = context.ActionDescriptor as ControllerActionDescriptor;
            var controllerType = controllerDescripter?.ControllerTypeInfo;
            if (controllerType == null)
                return Task.FromResult(0);

            var authorizationScope = GetAuthorizationScope(controllerType, context.HttpContext.RequestServices);
            if (authorizationScope == null)
                return Task.FromResult(0);
                
            return AddAuthorizationClaims(context.HttpContext.User, authorizationScope, context.HttpContext.RequestServices);
        }
    }
}
