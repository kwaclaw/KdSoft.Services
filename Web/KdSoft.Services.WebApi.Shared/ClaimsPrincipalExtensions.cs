using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using security = KdSoft.Services.Security;

namespace KdSoft.Services.WebApi
{
    public static class ClaimsPrincipalExtensions
    {
        public static IEnumerable<string> GetUserPermissions(this ClaimsPrincipal principal) {
            var claims = principal.FindAll(security.ClaimTypes.Permission);
            if (claims == null)
                throw new InvalidOperationException("Missing Permission Key.");
            
            return claims.Select(x => x.Value); 
        }

        public static int GetUserKey(this ClaimsPrincipal principal) {
            var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                throw new InvalidOperationException("Missing User Key.");
            return Int32.Parse(claim.Value);
        }

        public static int? GetUserKeyIfExists(this ClaimsPrincipal principal) {
            var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                return null;
            return Int32.Parse(claim.Value);
        }

        public static string GetUserName(this ClaimsPrincipal principal) {
            var claim = principal.FindFirst(ClaimTypes.Name);
            if (claim == null)
                throw new InvalidOperationException("Missing UserName.");
            return claim.Value;
        }
    }
}
