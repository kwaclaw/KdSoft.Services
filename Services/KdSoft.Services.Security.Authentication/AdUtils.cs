using KdSoft.Data.Models.Shared.Security;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Security;

namespace KdSoft.Services.Security
{
    public static class AdUtils
    {
        public static bool IsAdAuthType(string authType) {
            switch (authType.ToUpperInvariant()) {
                case "WINDOWS":
                case "KERBEROS":
                case "NEGOTIATE":
                case "NTLM":
                    return true;
                default:
                    return false;
            }
        }

        public static AdAccount ValidateAdUser(string adUserName, string adPassword) {
            string domain, userName;

            if (!AdAccount.TryParse(adUserName, out domain, out userName)) {
                throw new SecurityException("Invalid AD user name.");
            }

            PrincipalContext domainContext = null;
            try {
                try {
                    domainContext = new PrincipalContext(ContextType.Domain, domain, null);
                }
                catch (PrincipalServerDownException) {
                    domainContext = new PrincipalContext(ContextType.Machine, null);
                }

                if (domainContext.ValidateCredentials(userName, adPassword)) {
                    return new AdAccount { Domain = domainContext.GetDomainName(), UserName = userName };
                }
                else {
                    return null;
                }
            }
            finally {
                domainContext?.Dispose();
            }
        }

        public static (AdAccount account, ISet<AdAccount> groups) ValidateAdUserWithGroups(string adUserName, string adPassword) {
            string domain, userName;

            if (!AdAccount.TryParse(adUserName, out domain, out userName)) {
                throw new SecurityException("Invalid AD user name.");
            }

            PrincipalContext domainContext = null;
            try {
                try {
                    domainContext = new PrincipalContext(ContextType.Domain, domain, null);
                }
                catch (PrincipalServerDownException) {
                    domainContext = new PrincipalContext(ContextType.Machine, null);
                }

                if (domainContext.ValidateCredentials(userName, adPassword)) {
                    var securityGroups = GetAdSecurityGroups(domainContext, userName);
                    return (new AdAccount { Domain = domainContext.GetDomainName(), UserName = userName }, securityGroups);
                }
                else {
                    return (null, null);
                }
            }
            finally {
                domainContext?.Dispose();
            }
        }

        public static UserPrincipal GetUserPrincipal(AdAccount adAccount) {
            PrincipalContext domainContext = null;
            try {
                try {
                    domainContext = new PrincipalContext(ContextType.Domain, adAccount.Domain, null);
                }
                catch (PrincipalServerDownException) {
                    domainContext = new PrincipalContext(ContextType.Machine, null);
                }

                return UserPrincipal.FindByIdentity(domainContext, adAccount.UserName);
            }
            finally {
                domainContext?.Dispose();
            }
        }

        public static string GetDefaultADDomain() {
            PrincipalContext domainContext = null;
            try {
                try {
                    domainContext = new PrincipalContext(ContextType.Domain);
                }
                catch (PrincipalServerDownException) {
                    domainContext = new PrincipalContext(ContextType.Machine);
                }
                return GetDomainName(domainContext);
            }
            finally {
                domainContext?.Dispose();
            }
        }

        public static string GetDomainName(this PrincipalContext domainContext) {
            if (domainContext.ContextType == ContextType.Domain) {
                var dnsName = domainContext.ConnectedServer;
                int lastDotIndex = dnsName.LastIndexOf('.');
                if (lastDotIndex < 0)
                    return dnsName;
                int firstDotIndex = dnsName.IndexOf('.');
                if (firstDotIndex < 0)
                    return dnsName.Substring(0, lastDotIndex);
                firstDotIndex++;
                return dnsName.Substring(firstDotIndex, lastDotIndex - firstDotIndex);
            }
            else { //if (domainContext.ContextType == ContextType.Machine)
                return domainContext.ConnectedServer;
            }
        }

        public static ISet<AdAccount> GetAdSecurityGroups(this PrincipalContext domainContext, string userName) {
            var result = new HashSet<AdAccount>();
            var domainName = GetDomainName(domainContext);
            using (var user = UserPrincipal.FindByIdentity(domainContext, userName)) {
                using (var authGroups = user.GetAuthorizationGroups()) {
                    foreach (var authGroup in authGroups) {
                        result.Add(new AdAccount { Domain = domainName, UserName = authGroup.SamAccountName });
                    }
                }
            }

            return result;
        }

        public static ISet<AdAccount> GetAdSecurityGroups(AdAccount account) {
            PrincipalContext domainContext = null;
            try {
                try {
                    domainContext = new PrincipalContext(ContextType.Domain, account.Domain, null);
                }
                catch (PrincipalServerDownException) {
                    domainContext = new PrincipalContext(ContextType.Machine, null);
                }

                return GetAdSecurityGroups(domainContext, account.UserName);
            }
            finally {
                domainContext?.Dispose();
            }
        }
    }
}
