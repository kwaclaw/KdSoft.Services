using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace KdSoft.Services.WebApi.Infrastructure
{
    public class PermissionHandler: AuthorizationHandler<PermissionRequirement>, IPermissionConfig
    {
        ILogger logger;
        TimeSpan permissionsRefreshTime;
        long asyncToleranceTicks;  // how far can we go beyond expiry before we need to get the new roles right away

        TimeSpan IPermissionConfig.PermissionsRefreshTime { get { return permissionsRefreshTime; } }
        long IPermissionConfig.AsyncToleranceTicks { get { return asyncToleranceTicks; } }
        ILogger IPermissionConfig.Logger { get { return logger; } }

        public void Configure(TimeSpan permissionsRefreshTime) {
            this.permissionsRefreshTime = permissionsRefreshTime;
            asyncToleranceTicks = permissionsRefreshTime.Ticks / 3; // 33%
        }

        public PermissionHandler(ILoggerFactory loggerFactory) {
            logger = loggerFactory.CreateLogger(this.GetType());
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement) {
            var userRoles = context.User.FindAll(System.Security.Claims.ClaimTypes.Role);
            var permRoles = requirement.GetRoles(this);
            bool success = false;
            foreach (var ur in userRoles) {
                int roleIndx = permRoles.BinarySearch(ur.Value, StringComparer.Ordinal);
                if (roleIndx >= 0) {
                    success = true;
                    break;
                }
            }
            if (success)
                context.Succeed(requirement);
            else
                context.Fail();
            return Task.CompletedTask;
        }
    }
}
