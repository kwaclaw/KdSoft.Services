using Dapper;
using KdSoft.Data.Helpers;
using KdSoft.Data.Models.Security;
using KdSoft.Data.Models.Shared.Security;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace KdSoft.Data.Authentication
{
    public class AuthDb: Database<AuthDb>
    {
        public const string DefaultADDomainKey = "DefaultADDomain";

        public AuthDb() {
            this.Name = "AuthenticationDb";
        }

        const string userSql = @"
SELECT U.[UserId] AS UserKey, U.[UserName], U.[Password], U.[GivenName], U.[Surname], U.[EMail]
FROM sec.[User] U
WHERE U.[UserName] = @UserName AND U.[Inactive] = 0";

        public async Task<User> GetUser(string userName) {
            var users = await Conn.QueryAsync<User>(userSql, new { UserName = userName }).ConfigureAwait(false);
            return users.FirstOrDefault();
        }

        const string adUserSql = @"
SELECT U.[UserId] AS UserKey
FROM sec.[User] U INNER JOIN sec.[ADDomainUser] AU ON U.[UserId] = AU.[UserId]
    INNER JOIN sec.[OwnerSystem] OS ON AU.[OwnerId] = OS.[OwnerId]
WHERE AU.[Domain] = @Domain AND AU.[UserName] = @UserName AND U.[Inactive] = 0
    AND OS.[GlobalId] = ISNULL(@OwnerGuid, dbo.getDatabaseGuid())";

        public async Task<int?> GetAdUserKey(string domain, string userName, string ownerGuid = null) {
            var args = new { Domain = domain, UserName = userName, OwnerGuid = ownerGuid };
            var userKeys = await Conn.QueryAsync<int>(adUserSql, args).ConfigureAwait(false);
            using (var enumerator = userKeys.GetEnumerator()) {
                if (enumerator.MoveNext())
                    return enumerator.Current;
                else
                    return null;
            }
        }

        const string openIdUserSql = @"
SELECT U.[UserId] AS UserKey
FROM sec.[User] U INNER JOIN sec.[OpenIdUser] OU ON U.[UserId] = OU.[UserId]
INNER JOIN sec.[OpenIdIssuer] OI ON OU.[IssuerId] = OI.[IssuerId]
WHERE OI.[Identifier] = @Issuer AND OU.[OidSubject] = @Subject AND OI.[Inactive] = 0 AND U.[Inactive] = 0";

        public async Task<int?> GetOpenIdUserKey(string oidIssuer, string oidSubject) {
            var userKeys = await Conn.QueryAsync<int>(openIdUserSql, new { Issuer = oidIssuer, Subject = oidSubject }).ConfigureAwait(false);
            using (var enumerator = userKeys.GetEnumerator()) {
                if (enumerator.MoveNext())
                    return enumerator.Current;
                else
                    return null;
            }
        }

        const string userEmailSql = @"
SELECT U.[UserId] AS UserKey, U.[UserName], U.[Password], U.[GivenName], U.[Surname], U.[EMail]
FROM sec.[User] U
WHERE U.[EMail] = @Email AND U.[Inactive] = 0";

        public async Task<User> GetUserByEmail(string email) {
            var users = await Conn.QueryAsync<User>(userEmailSql, new { Email = email }).ConfigureAwait(false);
            return users.FirstOrDefault();
        }

        const string userKeySql = @"
SELECT U.[UserId] AS UserKey, U.[UserName], U.[Password], U.[GivenName], U.[Surname], U.[EMail]
FROM sec.[User] U
WHERE U.[UserId] = @UserKey AND U.[Inactive] = 0";

        public async Task<User> GetUserByKey(object userKey) {
            int key = (int)userKey;
            var users = await Conn.QueryAsync<User>(userKeySql, new { UserKey = key }).ConfigureAwait(false);
            return users.FirstOrDefault();
        }

        const string updatePwdSql = @"UPDATE sec.[User] SET [Password] = @Password WHERE [UserId] = @UserKey";

        public async Task<int> ChangePassword(User user, string newPwd) {
            using (var tx = Conn.BeginTransaction()) {
                var result = await Conn.ExecuteAsync(updatePwdSql, new { UserKey = user.UserKey, Password = newPwd }, tx).ConfigureAwait(false);
                tx.Commit();
                return result;
            }
        }

        const string checkOidIssuerSql = @"SELECT [IssuerId] FROM sec.[OpenIdIssuer] WHERE [Identifier] = @Issuer AND [Inactive] = 0";

        const string updateOidUserSql = @"
MERGE INTO sec.[OpenIdUser] U
USING (VALUES (@IssuerId, @Subject, @UserKey, @Email))
       AS S([IssuerId], [OidSubject], [UserId], [Email])
ON U.[IssuerId] = S.[IssuerId] AND U.[OidSubject] = S.[OidSubject] AND U.[UserId] = S.[UserId]
WHEN MATCHED THEN
    UPDATE
    SET U.[Email] = S.[Email]
WHEN NOT MATCHED THEN
    INSERT ( [IssuerId], [OidSubject], [UserId], [Email] )
    VALUES ( S.[IssuerId], S.[OidSubject], S.[UserId], S.[Email] );";

        public async Task<bool> LinkOpenIdAccount(int userKey, string issuer, string subject, string email) {
            bool result = false;
            using (var tx = Conn.BeginTransaction()) {

                var issuerIds = await Conn.QueryAsync<int>(checkOidIssuerSql, new { Issuer = issuer }, tx).ConfigureAwait(false);
                int issuerId;
                using (var enumerator = issuerIds.GetEnumerator()) {
                    if (enumerator.MoveNext())
                        issuerId = enumerator.Current;
                    else
                        throw new InvalidOperationException("OpenId Connect Issuer not supported");
                }

                var oidUser = new
                {
                    IssuerId = issuerId,
                    Subject = subject,
                    UserKey = userKey,
                    Email = string.IsNullOrEmpty(email) ? null : email
                };

                try {
                    int rowCount = await Conn.ExecuteAsync(updateOidUserSql, oidUser, tx).ConfigureAwait(false);
                    result = rowCount > 0;
                }
                catch (DbException ex) {
                    if (ex.Message != null && ex.Message.IndexOf("duplicate", StringComparison.OrdinalIgnoreCase) >= 0)
                        throw new InvalidOperationException("OpenID account already in use.");
                    else
                        throw;
                }

                tx.Commit();
            }
            return result;
        }

        const string removeOidUserSql = @"
DELETE U
FROM sec.[OpenIdUser] U INNER JOIN sec.[OpenIdIssuer] OI ON U.[IssuerId] = OI.[IssuerId]
WHERE OI.[Identifier] = @Issuer AND U.[OidSubject] = @Subject AND U.[UserId] = @UserKey";

        public async Task<bool> UnlinkOpenIdAccount(int userKey, string issuer, string subject) {
            bool result = false;
            using (var tx = Conn.BeginTransaction()) {
                var oidUser = new
                {
                    Issuer = issuer,
                    Subject = subject,
                    UserKey = userKey,
                };
                int rowCount = await Conn.ExecuteAsync(removeOidUserSql, oidUser, tx).ConfigureAwait(false);
                result = rowCount > 0;

                tx.Commit();
            }
            return result;
        }

        const string getOidUsersSql = @"
SELECT OI.[IDentifier] AS Issuer, U.[OidSubject] AS Subject, U.[Email]
FROM sec.[OpenIdUser] U INNER JOIN sec.[OpenIdIssuer] OI ON U.[IssuerId] = OI.[IssuerId]
WHERE U.[UserId] = @UserKey";

        public Task<IEnumerable<OpenIdAccount>> GetOpenIdAccounts(int userKey) {
            return Conn.QueryAsync<OpenIdAccount>(getOidUsersSql, new { UserKey = userKey });
        }
    }
}

