using System;

namespace KdSoft.Services.Security
{
    public interface IAuthorizationClaimsConfig: IClaimsCacheConfig
    {
        /// <summary>
        /// Time period after which authorization claims must be refreshed.
        /// </summary>
        TimeSpan ClaimsRefreshPeriod { get; }
    }
}
