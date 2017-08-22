using System;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Threading;
using KdSoft.Services.WebApi;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace KdSoft.Services.Security.Tests
{
    public class Tests
    {
        readonly ITestOutputHelper output;
        readonly FileTicketIssuer fileHelper;

        public Tests(ITestOutputHelper output) {
            this.output = output;
            var rootDir = ApplicationEnvironment.ApplicationBasePath;
            var builder = new ConfigurationBuilder().AddJsonFile(Path.Combine(rootDir, "security.json"));
            var config = builder.Build();

            // Load symmetric key for HMAC-SHA256 signature, must be 64 bytes
            var fileSettings = config.GetSection("File");
            string hexStr = fileSettings["AccessKey"];
            var keyBytes = KdSoft.Utils.Common.HexStrToBytes(hexStr);

            this.fileHelper = new FileTicketIssuer(keyBytes, TimeSpan.FromSeconds(3));
        }

        void CreateAndValidateTokenInternal(DateTimeOffset startTime, TimeSpan lifeTime, TimeSpan sleepTime) {
            var fileId = Guid.NewGuid();
            var expiry = startTime + lifeTime;
            var tokenStr = fileHelper.CreateFileAccessTicket(fileId, false, expiry);

            Thread.Sleep(sleepTime);

            var restoredFileId = fileHelper.ValidateFileAccessTicket(tokenStr, false);

            Assert.Equal<Guid>(fileId, restoredFileId);
        }

        [Fact]
        public void CreateAndValidateToken() {
            CreateAndValidateTokenInternal(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void CreateAndExpireToken() {
            Assert.Throws(typeof(ArgumentException), () => {
                CreateAndValidateTokenInternal(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3));
            });
        }

        [Fact]
        public void GetADSecurityGroupsMachine() {
            var domainContext = new PrincipalContext(ContextType.Machine, null);
            var adGroups = AdUtils.GetAdSecurityGroups(domainContext, "kwaclawek-dev\\kwaclaw");
            foreach (var adg in adGroups)
                output.WriteLine(adg.ToDownLevelName());
        }

        [Fact]
        public void GetADSecurityGroupsDomain() {
            var domainContext = new PrincipalContext(ContextType.Domain, "qlinesolutions.com", "testuser", "welcome");
            var adGroups = AdUtils.GetAdSecurityGroups(domainContext, "qlinesolutions\\testuser");
            foreach (var adg in adGroups)
                output.WriteLine(adg.ToDownLevelName());
        }
    }
}
