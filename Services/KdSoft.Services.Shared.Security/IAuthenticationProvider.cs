using KdSoft.Data.Models.Security;
using KdSoft.Data.Models.Shared.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KdSoft.Services.Security
{
    public interface IAuthenticationProvider: IDisposable
    {
        Task<int?> ValidateUser(string userName, string passWord);
        Task<AdAccount> ValidateAdUser(string adUserName, string adPassword, string adDomain = null);

        Task<int?> GetActiveDirectoryUserKey(AdAccount adAccount);
        string GetDefaultADDomain();
        string GetOwnerGuid();

        Task<User> GetUser(string userName);
        Task<User> GetUserByKey(int key);
        Task<int?> GetOpenIdUserKey(string oidIssuer, string oidSubject);

        Task<bool> ChangePassword(string userName, string oldPwd, string newPwd);
        Task<bool> LinkOpenIdAccount(int userKey, OpenIdAccount account);
        Task<bool> UnlinkOpenIdAccount(int userKey, string issuer, string subject);
        Task<IEnumerable<OpenIdAccount>> GetOpenIdAccounts(int userKey);
    }
}
