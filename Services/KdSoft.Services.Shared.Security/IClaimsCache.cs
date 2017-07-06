using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KdSoft.Services.Security
{
    public struct ClaimProperties
    {
        public IList<Claim> Claims { get; set; }
        public IList<byte[]> Properties { get; set; }
    }

    public enum ClaimIndexes: int
    {
        UserKey = 0,
        UserName = 1,
        AuthType = 2,
    };

    public enum PropertyOffsets: int
    {
        TokenValidFrom = 0,
        TokenValidTo = 1
    };

    public interface IClaimsCache
    {
        /// <summary>
        /// Returns the starting index for non-claim properties.
        /// </summary>
        int PropertiesStartIndex { get; }

        /// <summary>
        /// Caches standard claim and non-claim values and and returns them as claims and properties.
        /// </summary>
        /// <param name="claimsId">Key under which to store the values.</param>
        /// <param name="userName">Standard userName claim value.</param>
        /// <param name="authType">Standard authentication type claim value.</param>
        /// <param name="userKeyBytes">Encoded (serialized) standard userKey claim value. May be <c>null</c>.</param>
        /// <param name="tokenValidFrom">Standard token validFrom date-time property value.</param>
        /// <param name="tokenValidTo">Standard token validTo date-time property value.</param>
        /// <returns>Claim values as Claim instances, property values as byte arrays.</returns>
        Task<ClaimProperties> RetrieveAndCacheClaimPropertiesAsync(
            byte[] claimsId,
            string userName,
            string authType,
            byte[] userKeyBytes,
            DateTime tokenValidFrom = default(DateTime),
            DateTime tokenValidTo = default(DateTime)
        );

        /// <summary>
        /// Returns claims and non-claim properties.
        /// </summary>
        /// <param name="claimsId">Key for which to return the properties.</param>
        /// <param name="propIndexes">Indexes of properties to return, claims indexes must come before property indexes.
        /// If the value is <c>null</c> then all clainms and properties are returned.</param>
        /// <param name="claimsCount">Number of claims, also starting index of non-claim properties.</param>
        /// <returns>Lists off claims and properties for the given key.</returns>
        Task<ClaimProperties> GetClaimPropertyValuesAsync(byte[] claimsId, IList<int> propIndexes = null, int claimsCount = 0);

        /// <summary>
        /// Returns claims.
        /// </summary>
        /// <param name="claimsId">Key for which to return the properties.</param>
        /// <param name="propIndexes">Indexes of claim properties to return. Must not include non-claim indexes.
        /// If the value is <c>null</c> then all claims are returned.</param>
        /// <returns>List of claims for the given key.</returns>
        Task<IList<Claim>> GetClaimsAsync(byte[] claimsId, IList<int> propIndexes = null);

        /// <summary>
        /// Removes all entries for a given key.
        /// </summary>
        /// <param name="claimsId">Key to remove.</param>
        /// <returns><c>true</c> if entries for the key existed, <c>false</c> otherwise.</returns>
        Task<bool> RemoveClaimsAsync(byte[] claimsId);

        /// <summary>
        /// Determines if there are netries for a given key.
        /// </summary>
        /// <param name="claimsId">Key to check.</param>
        /// <returns><c>true</c> if entries for the key exist, <c>false</c> otherwise.</returns>
        bool ClaimsExist(byte[] claimsId);

        /// <summary>
        /// Returns user key converted to string. Should be used as the authoritative way
        /// to stringify the user key.
        /// </summary>
        /// <param name="userKeyBytes">Encoded user key.</param>
        string GetUserKeyString(byte[] userKeyBytes);
    }
}
