using KdSoft.Services.StorageServices;
using KdSoft.Services.StorageServices.Transient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using claims = System.Security.Claims;

namespace KdSoft.Services.Security
{
    public class BasicAuthClaimsCache: ClaimsCache, IClaimsCache
    {
        protected BasicAuthClaimsCache(string name, IClaimsCacheConfig config): base(name, config) {
            //do nothing
        }

        protected static string StringClaimDecode(byte[] bytes) {
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        protected static string Int32ClaimDecode(byte[] bytes) {
            int value = BitConverter.ToInt32(bytes, 0);
            return value.ToString();
        }

        protected override IList<ClaimDesc> GetClaimDescriptions() {
            // order of entries must maych enum ClaimIndexes
            var result = new List<ClaimDesc>() {
                new ClaimDesc( // UserId
                    new PropDesc(claims.ClaimTypes.NameIdentifier, typeof(Int32).FullName),
                    claims.ClaimValueTypes.Integer32,
                    Int32ClaimDecode),
                new ClaimDesc( // UserName
                    new PropDesc(claims.ClaimTypes.Name, typeof(string).FullName),
                    claims.ClaimValueTypes.String,
                    StringClaimDecode),
                new ClaimDesc(
                    new PropDesc(ClaimTypes.AuthType, typeof(string).FullName),
                    claims.ClaimValueTypes.String,
                    StringClaimDecode),
            };
            return result;
        }

        protected override IList<PropDesc> GetPropertyDescriptions() {
            // order of entries must maych enum PropertyOffsets
            var result = new List<PropDesc>() {
                new PropDesc("TokenValidFrom", "Int64"),
                new PropDesc("TokenValidTo", "Int64"),
            };
            return result;
        }

        protected virtual Task AddClaimsToBeCachedAsync(int userKey, List<PropertyValue> properties) {
            return Task.FromResult(0);
        }

        protected virtual Task AddPropertiesToBeCachedAsync(int userKey, List<PropertyValue> properties) {
            return Task.FromResult(0);
        }

        public async Task<ClaimProperties> RetrieveAndCacheClaimPropertiesAsync(
            byte[] claimsId,
            int? userKey,
            string userName,
            string authType,
            DateTime tokenValidFrom = default(DateTime),
            DateTime tokenValidTo = default(DateTime)
        ) {
            var propValues = new List<PropertyValue>() {
                new PropertyValue((int)ClaimIndexes.UserName, Encoding.UTF8.GetBytes(userName)),
                new PropertyValue((int)ClaimIndexes.AuthType, Encoding.UTF8.GetBytes(authType))
            };
            if (userKey != null) {
                propValues.Add(new PropertyValue((int)ClaimIndexes.UserKey, BitConverter.GetBytes(userKey.Value)));
                await AddClaimsToBeCachedAsync(userKey.Value, propValues).ConfigureAwait(false);
            }

            int claimsCount = propValues.Count;

            propValues.Add(new PropertyValue(PropertiesStartIndex + (int)PropertyOffsets.TokenValidFrom, BitConverter.GetBytes(tokenValidFrom.Ticks)));
            propValues.Add(new PropertyValue(PropertiesStartIndex + (int)PropertyOffsets.TokenValidTo, BitConverter.GetBytes(tokenValidTo.Ticks)));
            if (userKey != null) {
                await AddPropertiesToBeCachedAsync(userKey.Value, propValues).ConfigureAwait(false);
            }

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
    }
}
