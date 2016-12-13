using System.Security.Claims;
using System.Security.Principal;

namespace KdSoft.Services.Security.AspNet
{
    public static class SecurityConfig
    {
        public const string QlineAuthHeader = "X-QLine-Auth";
        public const string QlineAuthWindows = "Windows";

        public const string QlineAuthBasic = "QLine";
        public const string QlineAuthUid = "uid";
        public const string QlineAuthPwd = "pwd";

        public const string QlineAuthOpenId = "OpenId";
        public const string QlineAuthOpenIdIssuer = "iss";
        public const string QlineAuthOpenIdCode = "code";
        public const string QlineAuthOpenIdRedirectUri = "redir";

        public const string QlineRenewTokenHeader = "X-QLine-RenewToken";
        public const string QlineTokenKey = "X-QLine-Token";
        public const string QlineAuthOptionsHeader = "X-QLine-Auth-Options";
        public const string ActiveDirectoryClaimsIdPrefix = "#WU%";

        public static void Register(QLineAuthenticationOptions qLineAuthOptions) {
            QLineAuthOptions = qLineAuthOptions;

            // for the primary identity the priority order is: authentication type "X-QLine", then WindowsIdentity, then ClaimsIdentity
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

        public static QLineAuthenticationOptions QLineAuthOptions { get; private set; }
    }
}
