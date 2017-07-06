﻿using KdSoft.Services.StorageServices;
using KdSoft.Services.StorageServices.Transient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using claims = System.Security.Claims;

namespace KdSoft.Services.Security
{
    public abstract class BasicAuthClaimsCache: ClaimsCache, IClaimsCache
    {
        protected readonly ClaimDesc userKeyClaimDesc;

        protected BasicAuthClaimsCache(string name, IClaimsCacheConfig config, ClaimDesc userKeyClaimDesc): base(name, config) {
            this.userKeyClaimDesc = userKeyClaimDesc;
        }

        protected static string StringClaimDecode(byte[] bytes) {
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        protected static string Int32ClaimDecode(byte[] bytes) {
            int indx = 0;
            int value = Utils.Converter.ToInt32(bytes, ref indx);
            return value.ToString();
        }

        /// <inheritdoc/>
        protected override IList<ClaimDesc> GetClaimDescriptions() {
            // order of entries must maych enum ClaimIndexes
            var result = new List<ClaimDesc>() {
                userKeyClaimDesc,  // UserKey
                new ClaimDesc(     // UserName
                    new PropDesc(claims.ClaimTypes.Name, typeof(string).FullName),
                    claims.ClaimValueTypes.String,
                    StringClaimDecode),
                new ClaimDesc(     // AuthenticationType
                    new PropDesc(ClaimTypes.AuthType, typeof(string).FullName),
                    claims.ClaimValueTypes.String,
                    StringClaimDecode),
            };
            return result;
        }

        /// <inheritdoc/>
        protected override IList<PropDesc> GetPropertyDescriptions() {
            // order of entries must maych enum PropertyOffsets
            var result = new List<PropDesc>() {
                new PropDesc("TokenValidFrom", "Int64"),
                new PropDesc("TokenValidTo", "Int64"),
            };
            return result;
        }

        protected virtual Task AddClaimsToBeCachedAsync(string userName, string authType, byte[] userKeyBytes, List<PropertyValue> properties) {
            return Task.FromResult(0);
        }

        protected virtual Task AddPropertiesToBeCachedAsync(string userName, string authType, byte[] userKeyBytes, List<PropertyValue> properties) {
            return Task.FromResult(0);
        }

        /// <inheritdoc/>
        public async Task<ClaimProperties> RetrieveAndCacheClaimPropertiesAsync(
            byte[] claimsId,
            string userName,
            string authType,
            byte[] userKeyBytes,
            DateTime tokenValidFrom = default(DateTime),
            DateTime tokenValidTo = default(DateTime)
        ) {
            var propValues = new List<PropertyValue>() {
                new PropertyValue((int)ClaimIndexes.UserName, Encoding.UTF8.GetBytes(userName)),
                new PropertyValue((int)ClaimIndexes.AuthType, Encoding.UTF8.GetBytes(authType))
            };
            if (userKeyBytes != null) {
                propValues.Add(new PropertyValue((int)ClaimIndexes.UserKey, userKeyBytes));
            }
            await AddClaimsToBeCachedAsync(userName, authType, userKeyBytes, propValues).ConfigureAwait(false);

            int claimsCount = propValues.Count;

            propValues.Add(new PropertyValue(PropertiesStartIndex + (int)PropertyOffsets.TokenValidFrom, Utils.Converter.ToBytes(tokenValidFrom.Ticks)));
            propValues.Add(new PropertyValue(PropertiesStartIndex + (int)PropertyOffsets.TokenValidTo, Utils.Converter.ToBytes(tokenValidTo.Ticks)));
            await AddPropertiesToBeCachedAsync(userName, authType, userKeyBytes, propValues).ConfigureAwait(false);

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
        /// Returns claim and property values for a given cache entry identifier.
        /// </summary>
        /// <param name="claimsId">Identifier for cache entry.</param>
        /// <param name="propIndexes">Denotes the indexes of the values to return from the cache entry's stored values array.
        /// If <c>null</c> then this means all stored values.</param>
        /// <param name="claimsCount">Indicates how many of the entries in the <c>propIndexes</c> list are claim indexes, counted from the start.
        /// This argument is ignored when <c>propIndexes == null</c>.</param>
        /// <returns></returns>
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

        /// <inheritdoc/>
        public string GetUserKeyString(byte[] userKeyBytes) {
            return Int32ClaimDecode(userKeyBytes);
        }
    }
}
