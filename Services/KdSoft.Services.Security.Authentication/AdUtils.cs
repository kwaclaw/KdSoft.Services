using KdSoft.Data.Models.Shared.Security;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Security;

namespace KdSoft.Services.Security
{
    public static class AdUtils
    {
        public static AdAccount ValidateAdUser(string adUserName, string adPassword) {
            string domain, userName;

            if (!AdAccount.TryParse(adUserName, out domain, out userName)) {
                throw new SecurityException("Invalid AD user name.");
            }

            PrincipalContext domainContext = null;
            try {
                try {
                    domainContext = new PrincipalContext(ContextType.Domain, domain);
                }
                catch (PrincipalServerDownException) {
                    domainContext = new PrincipalContext(ContextType.Machine, null);
                }

                if (domainContext.ValidateCredentials(userName, adPassword)) {
                    return new AdAccount { Domain = domain, UserName = userName };
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
                    domainContext = new PrincipalContext(ContextType.Domain, domain);
                }
                catch (PrincipalServerDownException) {
                    domainContext = new PrincipalContext(ContextType.Machine, null);
                }

                if (domainContext.ValidateCredentials(userName, adPassword)) {
                    var securityGroups = GetAdSecurityGroups(domainContext, userName);
                    return (new AdAccount { Domain = domain, UserName = userName }, securityGroups);
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
                    domainContext = new PrincipalContext(ContextType.Domain, adAccount.Domain);
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
                catch (PrincipalServerDownException) {
                    domainContext = new PrincipalContext(ContextType.Machine);
                    return domainContext.ConnectedServer;
                }
            }
            finally {
                domainContext?.Dispose();
            }
        }

        public static ISet<AdAccount> GetAdSecurityGroups(PrincipalContext domainContext, string userName) {
            var result = new HashSet<AdAccount>();
            using (var user = UserPrincipal.FindByIdentity(domainContext, userName)) {
                using (var authGroups = user.GetAuthorizationGroups()) {
                    foreach (var authGroup in authGroups) {
                        result.Add(AdAccount.Parse(authGroup.SamAccountName));
                    }
                }
            }

            return result;
        }

        public static ISet<AdAccount> GetAdSecurityGroups(AdAccount account) {
            PrincipalContext domainContext = null;
            try {
                try {
                    domainContext = new PrincipalContext(ContextType.Domain, account.Domain);
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
