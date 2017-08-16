using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;

namespace KdSoft.Services.Security.AspNet
{
    public class KdSoftAuthenticationOptions: AuthenticationSchemeOptions
    {
        public KdSoftAuthenticationOptions() { }

        public string JwtAudience { get; set; }
        public string JwtIssuer { get; set; }
        public TimeSpan JwtLifeTime { get; set; }
        public SigningCredentials JwtCredentials { get; set; }

        public TokenValidationParameters ValidationParameters { get; set; }
        public JwtSecurityTokenHandler TokenHandler { get; set; }

        public List<OAuthClientCredentials> OAuthCredentials { get; set; }

        public IEnumerable<OAuthClientCredentials> GetOAuthCredentials(Func<OAuthClientCredentials, bool> predicate) {
            return OAuthCredentials.Where(predicate);
        }

        public IAuthenticationProvider AuthenticationProvider { get; set; }

        public IClaimsCache ClaimsCache { get; set; }
    }
}
