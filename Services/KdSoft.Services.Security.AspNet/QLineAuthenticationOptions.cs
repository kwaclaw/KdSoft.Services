using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KdSoft.Services.Security.AspNet
{
    public class QLineAuthenticationOptions: AuthenticationOptions
    {
        public QLineAuthenticationOptions() { }

        public string JwtAudience { get; set; }
        public string JwtIssuer { get; set; }
        public TimeSpan JwtLifeTime { get; set; }
        public SigningCredentials JwtCredentials { get; set; }

        public List<OAuthClientCredentials> OAuthCredentials { get; set; }

        public IEnumerable<OAuthClientCredentials> GetOAuthCredentials(Func<OAuthClientCredentials, bool> predicate) {
            return OAuthCredentials.Where(predicate);
        }

        public IAuthenticationProvider AuthenticationProvider { get; set; }

        public IClaimsCache ClaimsCache { get; set; }
    }
}
