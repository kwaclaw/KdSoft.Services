using KdSoft.Services.Security;
using System;

namespace KdSoft.Services.WebApi.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class AuthorizationScopeAttribute: Attribute
    {
        public AuthorizationScopeAttribute(Type interfaceType) {
            if (!typeof(IAuthorizationScope).IsAssignableFrom(interfaceType))
                throw new ArgumentException("Must derive from 'IAuthorizationScope'.", nameof(interfaceType));
            this.Type = interfaceType;
        }

        public Type Type { get; private set; }
    }
}
