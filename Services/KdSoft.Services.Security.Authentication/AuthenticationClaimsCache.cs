using KdSoft.Data.Models.Shared.Security;
using KdSoft.Services.StorageServices;
using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using claims = System.Security.Claims;

namespace KdSoft.Services.Security
{
    /// <summary>
    /// Default authentication claims cache, using <c>Int32</c> user key, email, surname and given name claims.
    /// </summary>
    public class AuthenticationClaimsCache: BasicAuthClaimsCache
    {
        public const string CacheName = "KdSoft.AuthenticationClaims";
        protected IAuthenticationProvider Provider { get; private set; }

        public AuthenticationClaimsCache(IClaimsCacheConfig config, IAuthenticationProvider provider) : base(CacheName, config, new PropsInitializer()) {
            this.Provider = provider;
        }

        protected class PropsInitializer: BasicPropertiesInitializer
        {
            public PropsInitializer() : base(
                new ClaimDesc(  // UserKey
                    new PropDesc(claims.ClaimTypes.NameIdentifier, typeof(Int32).FullName),
                    claims.ClaimValueTypes.Integer32,
                    Int32ClaimDecode)
            ) {
                //
            }

            public override IList<ClaimDesc> GetClaimDescriptions() {
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
        }

        void AddStringPropertyIfNotNull(List<PropertyValue> properties, string claimType, string value) {
            if (value != null) {
                var propValue = CreateStringPropValue(claimType, value);
                properties.Add(propValue);
            }
        }

        protected override async Task AddClaimsToBeCachedAsync(string userName, string authType, byte[] userKeyBytes, List<PropertyValue> properties) {
            await base.AddClaimsToBeCachedAsync(userName, authType, userKeyBytes, properties).ConfigureAwait(false);

            Data.Models.Security.User user = null;
            if (userKeyBytes != null) {
                int index = 0;
                var userKey = Utils.Converter.ToInt32(userKeyBytes, ref index);
                user = await Provider.GetUserByKey(userKey).ConfigureAwait(false);
                if (user == null) {
                    throw new SecurityException("User not found.");
                }

                AddStringPropertyIfNotNull(properties, claims.ClaimTypes.Email, user.Email);
                AddStringPropertyIfNotNull(properties, claims.ClaimTypes.Surname, user.Surname);
                AddStringPropertyIfNotNull(properties, claims.ClaimTypes.GivenName, user.GivenName);
            }

            // if we have an AD account, let's fill in missing properties from there
            if (user == null || user.Email == null || user.Surname == null || user.GivenName == null) {
                switch (authType) {
                    case claims.AuthenticationTypes.Windows:
                    case claims.AuthenticationTypes.Kerberos:
                    case claims.AuthenticationTypes.Negotiate:
                        string domain, uname;
                        if (AdAccount.TryParse(userName, out domain, out uname) && domain == null) {
                            domain = AdUtils.GetDefaultADDomain();
                        }
                        else {
                            break;
                        }
                        var adUser = AdUtils.GetUserPrincipal(new AdAccount { Domain = domain, UserName = uname });

                        if (user?.Email == null)
                            AddStringPropertyIfNotNull(properties, claims.ClaimTypes.Email, adUser.EmailAddress);
                        if (user?.Surname == null)
                            AddStringPropertyIfNotNull(properties, claims.ClaimTypes.Surname, adUser.Surname);
                        if (user?.GivenName == null)
                            AddStringPropertyIfNotNull(properties, claims.ClaimTypes.GivenName, adUser.GivenName);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
