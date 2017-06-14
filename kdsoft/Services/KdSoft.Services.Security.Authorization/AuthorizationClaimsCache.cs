using KdSoft.Data.Models.Security;
using KdSoft.Services.StorageServices;
using KdSoft.Services.StorageServices.Transient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using claims = System.Security.Claims;

namespace KdSoft.Services.Security
{
    public class AuthorizationClaimsCache: ClaimsCache
    {
        protected IAuthorizationProvider Provider { get; private set; }

        public AuthorizationClaimsCache(
            string name,
            IClaimsCacheConfig config,
            IAuthorizationProvider provider
        ) : base(name, config) {
            this.Provider = provider;
        }

        static readonly char[] splitSep = new[] { ',' };

        protected static string[] StringSplitClaimDecode(byte[] bytes) {
            var tmp = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            return tmp.Split(splitSep, StringSplitOptions.RemoveEmptyEntries);
        }

        protected override IList<ClaimDesc> GetClaimDescriptions() {
            var result = new List<ClaimDesc>();
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

        protected override IList<PropDesc> GetPropertyDescriptions() {
            return new List<PropDesc>();
        }

        protected virtual async Task AddClaimsToBeCachedAsync(int userKey, List<PropertyValue> properties) {
            var strBuilder = new StringBuilder();
            var roles = await Provider.GetUserRoles(userKey).ConfigureAwait(false);
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

        protected virtual Task AddPropertiesToBeCachedAsync(int userKey, List<PropertyValue> properties) {
            return Task.FromResult(0);
        }

        public async Task<ClaimProperties> RetrieveAndCacheClaimPropertiesAsync(int userKey) {
            var propValues = new List<PropertyValue>();

            await AddClaimsToBeCachedAsync(userKey, propValues).ConfigureAwait(false);
            int claimsCount = propValues.Count;
            await AddPropertiesToBeCachedAsync(userKey, propValues).ConfigureAwait(false);

            var claimsId = BitConverter.GetBytes(userKey);
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

        public async Task<ClaimProperties> GetClaimPropertyValuesAsync(int userKey, IList<int> propIndexes = null, int claimsCount = 0) {
            var claimsId = BitConverter.GetBytes(userKey);
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

    }
}
