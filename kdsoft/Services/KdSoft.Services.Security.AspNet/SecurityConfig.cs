using System.Security.Claims;
using System.Security.Principal;

namespace KdSoft.Services.Security.AspNet
{
    public static class SecurityConfig
    {
        public const string AuthHeader = "X-KdSoft-Auth";
        public const string AuthWindows = "Windows";

        public const string AuthCustom = "KdSoft";
        public const string AuthUid = "uid";
        public const string AuthPwd = "pwd";

        public const string AuthOpenId = "OpenId";
        public const string AuthOpenIdIssuer = "iss";
        public const string AuthOpenIdCode = "code";
        public const string AuthOpenIdRedirectUri = "redir";

        public const string RenewTokenHeader = "X-KdSoft-RenewToken";
        public const string TokenKey = "X-KdSoft-Token";
        public const string AuthOptionsHeader = "X-KdSoft-Auth-Options";
        public const string ActiveDirectoryClaimsIdPrefix = "#WU%";

        public static void Register(KdSoftAuthenticationOptions authOptions) {
            KdSoftAuthOptions = authOptions;

            // for the primary identity the priority order is: authentication type "X-KdSoft", then WindowsIdentity, then ClaimsIdentity
            ClaimsPrincipal.PrimaryIdentitySelector = (ids) => {
                ClaimsIdentity result = null;
                WindowsIdentity winIdentity = null;

                using (var enumerator = ids.GetEnumerator()) {
                    while (enumerator.MoveNext()) {
                        var identity = enumerator.Current;
                        if (identity == null)
                            continue;
                        if (AuthenticationContext.IsValidAuthRequestTypeName(identity.AuthenticationType)) {
                            return identity;
                        }
                        if (winIdentity == null) {
                            winIdentity = identity as WindowsIdentity;
                            if (winIdentity == null && result == null)
                                result = identity;
                        }
                    }
                }
                if (winIdentity != null)
                    return winIdentity;
                else
                    return result;
            };
        }

        public static KdSoftAuthenticationOptions KdSoftAuthOptions { get; private set; }
    }
}
