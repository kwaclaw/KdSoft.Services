using System;

namespace KdSoft.Services.Security
{
    /// <summary>
    /// Attribute to mark a class for using a specific <see cref="IAuthorizationScope"/> implementation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class AuthorizationScopeAttribute: Attribute
    {
        public AuthorizationScopeAttribute(Type interfaceType) {
            if (!typeof(IAuthorizationScope).IsAssignableFrom(interfaceType))
                throw new ArgumentException("Must derive from 'IAuthorizationScope'.", nameof(interfaceType));
            this.Type = interfaceType;
        }

        /// <summary>
        /// Type of <see cref="IAuthorizationScope"/> implementation to use.
        /// </summary>
        public Type Type { get; private set; }
    }
}
