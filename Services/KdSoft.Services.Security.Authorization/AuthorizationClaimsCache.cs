using KdSoft.Data.Models.Security;
using KdSoft.Data.Models.Shared.Security;
using KdSoft.Services.StorageServices;
using KdSoft.Services.StorageServices.Transient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using claims = System.Security.Claims;

namespace KdSoft.Services.Security
{
    /// <summary>
    /// Default authorization claims cache.
    /// </summary>
    public class AuthorizationClaimsCache: ClaimsCache
    {
        protected IAuthorizationProvider Provider { get; private set; }
        protected TimeSpan RefreshPeriod { get; private set; }

        public AuthorizationClaimsCache(
            string name,
            IAuthorizationClaimsConfig config,
            IAuthorizationProvider provider
        ) : base(name, config, new PropsInitializer()) {
            this.RefreshPeriod = config.ClaimsRefreshPeriod;
            this.Provider = provider;
        }

        static readonly char[] splitSep = new[] { ',' };

        protected static string[] StringSplitClaimDecode(byte[] bytes) {
            var tmp = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            return tmp.Split(splitSep, StringSplitOptions.RemoveEmptyEntries);
        }

        protected static string DateTimeOffsetUtcClaimDecode(byte[] bytes) {
            int index = 0;
            var ticks = Utils.Converter.ToInt64(bytes, ref index);
            var dt = new DateTimeOffset(ticks, TimeSpan.Zero);
            return dt.ToString("o");
        }

        /// <inheritdoc />
        protected class PropsInitializer: PropertiesInitializer
        {
            /// <inheritdoc />
            public override IList<ClaimDesc> GetClaimDescriptions() {
                var result = new List<ClaimDesc>();
                result.Add(new ClaimDesc(
                    new PropDesc(ClaimTypes.AuthTimeUtc, typeof(DateTimeOffset).FullName),
                    claims.ClaimValueTypes.DateTime,
                    DateTimeOffsetUtcClaimDecode,
                    null));
                result.Add(new ClaimDesc(
                    new PropDesc(ClaimTypes.AdSecurityGroup, typeof(AdAccount[]).FullName),
                    claims.ClaimValueTypes.String,
                    null,
                    StringSplitClaimDecode));
                result.Add(new ClaimDesc(
                    new PropDesc(claims.ClaimTypes.Role, typeof(Role[]).FullName),
                    claims.ClaimValueTypes.String,
                    null,
                    StringSplitClaimDecode));
                result.Add(new ClaimDesc(
                    new PropDesc(ClaimTypes.Permission, typeof(Permission[]).FullName),
                    claims.ClaimValueTypes.String,
                    null,
                    StringSplitClaimDecode));
                return result;
            }

            /// <inheritdoc />
            public override IList<PropDesc> GetPropertyDescriptions() {
                return new List<PropDesc>();
            }
        }

        /// <summary>
        /// Uses the configured authorization provider and Active Directory (if applicable) to retrieve
        /// authorization information and add it to the <see param="properties"/> list passed as argument.
        /// </summary>
        /// <param name="userName">User name component.</param>
        /// <param name="authType">Authentication type component.</param>
        /// <param name="userKey">User key component.</param>
        /// <param name="properties">List to add authorization properties to.</param>
        protected virtual async Task AddClaimsToBeCachedAsync(string userName, string authType, int? userKey, List<PropertyValue> properties) {
            StringBuilder strBuilder = null;

            // mandatory property, its presence allows us to check if authorization claims have been loaded
            properties.Add(CreatePropValue(ClaimTypes.AuthTimeUtc, Utils.Converter.ToBytes(DateTimeOffset.UtcNow.Ticks)));

            // if we have an AD account, add the AD security groups as claims
            switch (authType) {
                case claims.AuthenticationTypes.Windows:
                case claims.AuthenticationTypes.Kerberos:
                case claims.AuthenticationTypes.Negotiate:
                    string domain, uname;
                    if (AdAccount.TryParse(userName, out domain, out uname)) {
                        if (domain == null)
                            domain = AdUtils.GetDefaultADDomain();
                    }
                    else {
                        break;
                    }
                    var adGroups = AdUtils.GetAdSecurityGroups(new AdAccount { Domain = domain, UserName = uname });

                    strBuilder = strBuilder ?? new StringBuilder();
                    foreach (var adg in adGroups)
                        strBuilder.Append(adg.ToDownLevelName() + ",");
                    if (strBuilder.Length > 0)
                        strBuilder.Remove(strBuilder.Length - 1, 1); // remove last comma
                    properties.Add(CreateStringPropValue(ClaimTypes.AdSecurityGroup, strBuilder.ToString()));
                    break;
                default:
                    break;
            }

            // for a registered application user we add the roles and permissions defined in the application
            if (userKey != null) {
                strBuilder = strBuilder ?? new StringBuilder();
                var roles = await Provider.GetUserRoles(userKey.Value).ConfigureAwait(false);
                strBuilder.Clear();
                foreach (var role in roles)
                    strBuilder.Append(role.RoleKey + ",");
                if (strBuilder.Length > 0)
                    strBuilder.Remove(strBuilder.Length - 1, 1); // remove last comma
                properties.Add(CreateStringPropValue(claims.ClaimTypes.Role, strBuilder.ToString()));

                var permissions = await Provider.GetRolePermissions(roles.Select(r => r.RoleKey)).ConfigureAwait(false);
                strBuilder.Clear();
                foreach (var perm in permissions)
                    strBuilder.Append(perm.PermKey + ",");
                if (strBuilder.Length > 0)
                    strBuilder.Remove(strBuilder.Length - 1, 1); // remove last comma
                properties.Add(CreateStringPropValue(ClaimTypes.Permission, strBuilder.ToString()));
            }
        }

        /// <summary>
        /// Uses the configured authorization provider and Active Directory (if applicable) to retrieve
        /// non-claim authorization information and add it to the <see param="properties"/> list passed as argument.
        /// </summary>
        /// <param name="userName">User name component.</param>
        /// <param name="authType">Authentication type component.</param>
        /// <param name="userKey">User key component.</param>
        /// <param name="properties">List to add authorization properties to.</param>
        protected virtual Task AddPropertiesToBeCachedAsync(string userName, string authType, int? userKey, List<PropertyValue> properties) {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Builds claims identifier from components.
        /// </summary>
        /// <param name="userName">User name component.</param>
        /// <param name="authType">Authentication type component.</param>
        /// <param name="userKey">User key component.</param>
        /// <returns>Byte buffer representing identifier for cache entry.</returns>
        public byte[] GetClaimsId(string userName, string authType, int? userKey) {
            var result = new byte[512];
            var byteCount = Encoding.UTF8.GetBytes(userName, 0, userName.Length, result, 0);
            byteCount += Encoding.UTF8.GetBytes(authType, 0, authType.Length, result, byteCount);
            if (userKey != null) {
                Utils.Converter.ToBytes(userKey.Value, result, ref byteCount);
            }
            Array.Resize<byte>(ref result, byteCount);
            return result;
        }

        /// <summary>
        /// Loads authorization information and returns it in the form of claims and property buffers.
        /// </summary>
        /// <param name="userName">User name for which to load authorization information.</param>
        /// <param name="authType">Authentication type used to validate user identity.</param>
        /// <param name="userKey">Optional user key for data storage lookups.</param>
        /// <returns>Lists of claims and property buffers.</returns>
        public async Task<ClaimProperties> RetrieveAndCacheClaimPropertiesAsync(string userName, string authType, int? userKey) {
            var propValues = new List<PropertyValue>();

            await AddClaimsToBeCachedAsync(userName, authType, userKey, propValues).ConfigureAwait(false);
            int claimsCount = propValues.Count;
            await AddPropertiesToBeCachedAsync(userName, authType, userKey, propValues).ConfigureAwait(false);

            var claimsId = GetClaimsId(userName, authType, userKey);
            var propEntries = await StorePropertyValuesAsync(claimsId, propValues).ConfigureAwait(false);

            var claims = CreateClaims(new ArraySegment<PropEntry>(propEntries.Array, propEntries.Offset, claimsCount));

            var properties = new List<byte[]>(propEntries.Count - claimsCount);
            int limit = propEntries.Offset + propEntries.Count;
            for (int indx = propEntries.Offset + claimsCount; indx < limit; indx++) {
                properties.Add(propEntries.Array[indx].Value);
            }

            return new ClaimProperties
            {
                Claims = claims,
                Properties = properties
            };
        }

        /// <summary>
        /// Retrieves claims and properties for a given claims identifier.
        /// </summary>
        /// <param name="claimsId">Identifier for cache entry.</param>
        /// <param name="propIndexes">List of indexes for property values stored in cache entry.</param>
        /// <param name="claimsCount">Number of property values that are claims, counted from the start of the propIndexes list.</param>
        /// <returns>Lists of claims and property buffers.</returns>
        public async Task<ClaimProperties> GetClaimPropertyValuesAsync(byte[] claimsId, IList<int> propIndexes = null, int claimsCount = 0) {
            var propEntries = await GetPropertyValuesAsync(claimsId, propIndexes).ConfigureAwait(false);

            int propCount;
            if (propIndexes == null) {
                claimsCount = PropertiesStartIndex;
                propCount = Store.PropDescs.Length - claimsCount;
            }
            else {
                propCount = propIndexes.Count - claimsCount;
                if (propCount < 0)
                    throw new ArgumentException("Invalid claims count.", "claimsCount");
            }

            var claims = CreateClaims(new ArraySegment<PropEntry>(propEntries.Array, propEntries.Offset, claimsCount));

            var properties = new List<byte[]>(propCount);
            int limit = propEntries.Offset + propEntries.Count;
            for (int indx = propEntries.Offset + claimsCount; indx < limit; indx++) {
                properties.Add(propEntries.Array[indx].Value);
            }

            return new ClaimProperties
            {
                Claims = claims,
                Properties = properties
            };
        }

        /// <summary>
        /// Retrieves or refreshes claims and properties for a given set of identifier components.
        /// Refresh is based on <see cref="IAuthorizationClaimsConfig.ClaimsRefreshPeriod"/>.
        /// </summary>
        /// <param name="userName">User name for which to load authorization information.</param>
        /// <param name="authType">Authentication type used to validate user identity.</param>
        /// <param name="userKey">Optional user key for data storage lookups.</param>
        /// <returns>Lists of claims and property buffers.</returns>
        public async Task<ClaimProperties> GetClaimPropertyValuesAsync(string userName, string authType, int? userKey) {
            ClaimProperties claimsResult;

            var claimsId = GetClaimsId(userName, authType, userKey);
            bool haveClaims = ClaimsExist(claimsId);

            if (haveClaims) {
                claimsResult = await GetClaimPropertyValuesAsync(claimsId).ConfigureAwait(false);
                // if no claims were found then they probably got removed immediately before the call
                haveClaims = claimsResult.Claims != null && claimsResult.Claims.Count > 0;
                if (haveClaims) {
                    var tempClaims = claimsResult.Claims;
                    var authTimeClaim = tempClaims.FirstOrDefault(claim => claim.Type == KdSoft.Services.Security.ClaimTypes.AuthTimeUtc);
                    if (authTimeClaim == null)
                        throw new SecurityException("Authorization claims invalid.");

                    // now lets check if the claims need to be refreshed
                    var authTime = DateTimeOffset.Parse(authTimeClaim.Value, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    haveClaims = (authTime + RefreshPeriod) > DateTimeOffset.UtcNow;
                }
            }

            if (!haveClaims) { // if no active claims are in the cache then we need to (re-)load the authorization claims
                claimsResult = await RetrieveAndCacheClaimPropertiesAsync(userName, authType, userKey).ConfigureAwait(false);
            }

            return claimsResult;
        }
    }
}
