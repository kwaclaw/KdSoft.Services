using KdSoft.Services.StorageServices;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using claims = System.Security.Claims;

namespace KdSoft.Services.Security
{
    public class AuthenticationClaimsCache: BasicAuthClaimsCache
    {
        public const string CacheName = "KdSoft.AuthenticationClaims";
        protected IAuthenticationProvider Provider { get; private set; }

        public AuthenticationClaimsCache(IClaimsCacheConfig config, IAuthenticationProvider provider) : base(CacheName, config) {
            this.Provider = provider;
        }

        static readonly char[] splitSep = new[] { ',' };

        protected override IList<ClaimDesc> GetClaimDescriptions() {
            var result = base.GetClaimDescriptions();
            result.Add(new ClaimDesc(
                new PropDesc(claims.ClaimTypes.Email, typeof(string).FullName),
                claims.ClaimValueTypes.String,
                StringClaimDecode));
            result.Add(new ClaimDesc(
                new PropDesc(claims.ClaimTypes.Surname, typeof(string).FullName),
                claims.ClaimValueTypes.String,
                StringClaimDecode));
            result.Add(new ClaimDesc(
                new PropDesc(claims.ClaimTypes.GivenName, typeof(string).FullName),
                claims.ClaimValueTypes.String,
                StringClaimDecode));
            return result;
        }

        void AddStringPropertyIfNotNull(List<PropertyValue> properties, string claimType, string value) {
            if (value != null) {
                var propValue = CreateStringPropValue(claimType, value);
                properties.Add(propValue);
            }
        }

        protected override async Task AddClaimsToBeCachedAsync(int userKey, List<PropertyValue> properties) {
            await base.AddClaimsToBeCachedAsync(userKey, properties);

            var user = await Provider.GetUserByKey(userKey).ConfigureAwait(false);
            if (user == null) {
                throw new SecurityException("User not found.");
            }

            AddStringPropertyIfNotNull(properties, claims.ClaimTypes.Email, user.Email);
            AddStringPropertyIfNotNull(properties, claims.ClaimTypes.Surname, user.Surname);
            AddStringPropertyIfNotNull(properties, claims.ClaimTypes.GivenName, user.GivenName);
        }
    }
}
