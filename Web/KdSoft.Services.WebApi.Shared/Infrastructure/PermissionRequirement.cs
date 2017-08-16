using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KdSoft.Data.Models.Security;
using KdSoft.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace KdSoft.Services.WebApi.Infrastructure
{
    public interface IPermissionConfig {
        TimeSpan PermissionsRefreshTime { get; }
        long AsyncToleranceTicks { get; }  // how far can we go beyond expiry before we need to get the new roles right away
        ILogger Logger { get; }
    }

    public class PermissionRequirement: IAuthorizationRequirement
    {
        IAuthorizationProvider provider;
        bool updatingRoles;
        DateTimeOffset rolesExpiry;
        List<string> roles;
        readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

        public PermissionRequirement(string permission, IAuthorizationProvider provider) {
            this.provider = provider;
            this.Permission = permission;
        }

        public string Permission { get; private set; }

        #region Roles Implementation

        async Task<List<string>> GetNewRoles(string[] permissions) {
            var permRoles = await provider.GetPermissionRoles(permissions).ConfigureAwait(false);
            var result = permRoles.Select<Role, string>(r => r.RoleKey).ToList();
            result.Sort(StringComparer.Ordinal);
            return result;
        }


        void GetNewRolesInBackground(IPermissionConfig config) {
            updatingRoles = true;
            try {
                var getTask = GetNewRoles(new[] { Permission });
                getTask.ContinueWith(gt => {
                    try {
                        rwLock.EnterWriteLock();
                        try {
                            updatingRoles = false;
                            roles = gt.Result;
                            rolesExpiry = DateTimeOffset.UtcNow + config.PermissionsRefreshTime;
                        }
                        finally {
                            rwLock.ExitWriteLock();
                        }
                    }
                    catch (Exception ex) {
                        var logger = config.Logger;
                        if (logger != null) {
                            logger.LogError("Error getting permission roles.", ex);
                        }
                    }
                });
            }
            catch {
                rwLock.EnterWriteLock();
                try {
                    updatingRoles = false;
                }
                finally {
                    rwLock.ExitWriteLock();
                }
                throw;
            }
        }

        // run this under lock protection
        void CheckUpdate(long asyncToleranceTicks, out bool updateRoles, out bool runBackgroundJob) {
            updateRoles = false;
            runBackgroundJob = false;

            if (roles == null) {
                updateRoles = true;
            }
            else if (!updatingRoles) {
                var now = DateTimeOffset.UtcNow;
                var pastExpiryTicks = (now - rolesExpiry).Ticks;
                // if we are too far beyond expiry let's refresh the roles now
                if (pastExpiryTicks > asyncToleranceTicks) {
                    updateRoles = true;
                }
                // otherwise let's use the current roles and start a background job to refresh them
                if (pastExpiryTicks > 0) {
                    runBackgroundJob = true;
                }
            }
        }

        public List<string> GetRoles(IPermissionConfig config) {
            bool updateRoles, runBackgroundJob;

            rwLock.EnterReadLock();
            try {
                CheckUpdate(config.AsyncToleranceTicks, out updateRoles, out runBackgroundJob);
            }
            finally {
                rwLock.ExitReadLock();
            }

            if (!updateRoles && !runBackgroundJob) {
                return roles;
            }

            // double lock check 
            rwLock.EnterUpgradeableReadLock();
            try {
                CheckUpdate(config.AsyncToleranceTicks, out updateRoles, out runBackgroundJob);
                if (!updateRoles && !runBackgroundJob) {
                    return roles;
                }

                rwLock.EnterWriteLock();
                try {
                    if (runBackgroundJob) {
                        GetNewRolesInBackground(config);
                    }
                    else {
                        roles = GetNewRoles(new[] { Permission }).Result;
                        rolesExpiry = DateTimeOffset.UtcNow + config.PermissionsRefreshTime;
                    }
                    return roles;
                }
                finally {
                    rwLock.ExitWriteLock();
                }
            }
            finally {
                rwLock.ExitUpgradeableReadLock();
            }
        }

        #endregion
    }
}
