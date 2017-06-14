using KdSoft.Data.Models.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KdSoft.Services.Security
{
    public interface IAuthorizationProvider: IDisposable
    {
        Task<IEnumerable<Permission>> GetUserPermissions(int userKey);
        Task<IEnumerable<RoleInfo>> GetUserRoles(string userName);
        Task<IEnumerable<RoleInfo>> GetUserRoles(int userKey);
        Task<IEnumerable<Permission>> GetRolePermissions(IEnumerable<string> roleKeys);
        Task<IEnumerable<Role>> GetPermissionRoles(IEnumerable<string> permCodes);
    }
}
