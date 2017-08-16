using System;
using Microsoft.AspNetCore.Authorization;

namespace KdSoft.Services.WebApi.Infrastructure
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class PermissionAuthorizeAttribute: AuthorizeAttribute
    {
        public PermissionAuthorizeAttribute(string permission) : base(permission) { }
    }
}
