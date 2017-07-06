using System;

namespace KdSoft.Data.Models.Shared.Security
{
    public class ChangePasswordModel
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class RegisterModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoggedInAs
    {
        public string UserName { get; set; }
        public bool IsAuthenticated { get; set; }
    }

    /// <summary>
    /// Represents Active Directory account (user or group).
    /// </summary>
    public class AdAccount: IEquatable<AdAccount>
    {
        public string Domain { get; set; }
        public string UserName { get; set; }

        // Parses an AD user name into its components. Does not perform full validation of characters.
        // Returns true when either exactly one '\' or '@' character is found, or none of them.
        // In the latter case the domain will be returned as null.
        public static bool TryParse(string adUserName, out string domain, out string userName) {
            int slashIndex;
            if ((slashIndex = adUserName.IndexOf('\\')) == adUserName.LastIndexOf('\\') && slashIndex >= 0) {
                domain = adUserName.Substring(0, slashIndex);
                userName = adUserName.Substring(slashIndex + 1);
                return true;
            }

            int atIndex;
            if ((atIndex = adUserName.IndexOf('@')) == adUserName.LastIndexOf('@') && atIndex >= 0) {
                userName = adUserName.Substring(0, atIndex);
                domain = adUserName.Substring(atIndex + 1);
                return true;
            }

            if (atIndex < 0 && slashIndex < 0) {
                userName = adUserName;
                domain = null;
                return true;
            }

            userName = null; ;
            domain = null;
            return false;
        }

        public static bool TryParse(string adUserName, out AdAccount adAccount) {
            string domain, userName;
            bool result = TryParse(adUserName, out domain, out userName);
            if (result)
                adAccount = new AdAccount { Domain = domain, UserName = userName };
            else
                adAccount = null;
            return result;
        }

        public static AdAccount Parse(string adUserName) {
            string domain, userName;
            if (!TryParse(adUserName, out domain, out userName)) {
                throw new ArgumentException("Invalid AD user name", nameof(adUserName));
            }
            return new AdAccount { Domain = domain, UserName = userName };
        }

        public string ToSamAccountName() {
            return string.IsNullOrEmpty(Domain) ? UserName : Domain + '\\' + UserName;
        }

        public string ToUserPrincipalName() {
            return UserName + (string.IsNullOrEmpty(Domain) ? string.Empty : '@' + Domain);
        }

        public override int GetHashCode() {
            return (Domain?.GetHashCode() ?? 0) ^ (UserName?.GetHashCode() ?? 0);
        }

        public override bool Equals(object obj) {
            return this.Equals(obj as AdAccount);
        }

        public bool Equals(AdAccount other) {
            if (other == null)
                return false;
            var comparer = StringComparer.CurrentCultureIgnoreCase;
            int result = comparer.Compare(this.Domain, other.Domain);
            if (result == 0)
                result = comparer.Compare(this.UserName, other.UserName);
            return result == 0;
        }

        public static bool operator ==(AdAccount x, AdAccount y) {
            return x.Equals(y);
        }

        public static bool operator !=(AdAccount x, AdAccount y) {
            return !x.Equals(y);
        }
    }

    public class OpenIdAuthorization
    {
        public string Issuer { get; set; }
        public string AuthorizationCode { get; set; }
        public string RedirectUri { get; set; }
    }

    public class OpenIdAccount
    {
        public string Issuer { get; set; }
        public string Subject { get; set; }
        public string Email { get; set; }
    }

    public class OAuthClientAppId
    {
        public string Issuer { get; set; }
        public string ClientId { get; set; }
        public string Application { get; set; }
    }
}
