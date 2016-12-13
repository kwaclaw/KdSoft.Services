using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QLine.Services.WebApi.Infrastructure
{
    public class ClaimsCultureProvider: RequestCultureProvider
    {
        public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext) {
            var user = httpContext.User;
            var claimCultures = user.Claims.Where(c => c.Type == ClaimTypes.Locality);
            ProviderCultureResult result = null;
            if (claimCultures.Any())
                result = new ProviderCultureResult(claimCultures.Select(cc => cc.Value).ToList());
            return Task.FromResult(result);
        }
    }
}
