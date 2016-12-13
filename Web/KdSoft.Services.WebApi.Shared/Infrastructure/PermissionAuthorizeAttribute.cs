using Microsoft.AspNetCore.Authorization;
using System;

namespace QLine.Services.WebApi.Infrastructure
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class PermissionAuthorizeAttribute: AuthorizeAttribute
    {
        public PermissionAuthorizeAttribute(string permission) : base(permission) { }
    }
}
