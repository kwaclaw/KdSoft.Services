﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using security = System.Security.Claims;

namespace KdSoft.Services.Security
{
    public static class ClaimsPrincipalExtensions
    {
        public static IEnumerable<string> GetUserPermissions(this ClaimsPrincipal principal) {
            var claims = principal.FindAll(ClaimTypes.Permission);
            if (claims == null)
                throw new InvalidOperationException("Missing Permission Key.");
            
            return claims.Select(x => x.Value); 
        }

        public static int GetUserKey(this ClaimsPrincipal principal) {
            var claim = principal.FindFirst(security.ClaimTypes.NameIdentifier);
            if (claim == null)
                throw new InvalidOperationException("Missing User Key.");
            return Int32.Parse(claim.Value);
        }

        public static int? GetUserKeyIfExists(this ClaimsPrincipal principal) {
            var claim = principal.FindFirst(security.ClaimTypes.NameIdentifier);
            if (claim == null)
                return null;
            return Int32.Parse(claim.Value);
        }

        public static string GetUserName(this ClaimsPrincipal principal) {
            var claim = principal.FindFirst(security.ClaimTypes.Name);
            if (claim == null)
                throw new InvalidOperationException("Missing UserName.");
            return claim.Value;
        }

        public static string GetUserAuthType(this ClaimsPrincipal principal) {
            var claim = principal.FindFirst(ClaimTypes.AuthType);
            if (claim == null)
                throw new InvalidOperationException("Missing AuthType.");
            return claim.Value;
        }

        public static string GetUserAuthTypeIfExists(this ClaimsPrincipal principal) {
            var claim = principal.FindFirst(ClaimTypes.AuthType);
            return claim?.Value;
        }
    }
}
