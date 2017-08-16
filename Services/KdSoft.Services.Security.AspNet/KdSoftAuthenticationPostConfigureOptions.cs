using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace KdSoft.Services.Security.AspNet
{
    /// <summary>
    /// Used to setup defaults for all <see cref="KdSoftAuthenticationOptions"/>.
    /// </summary>
    public class KdSoftAuthenticationPostConfigureOptions: IPostConfigureOptions<KdSoftAuthenticationOptions>
    {
        /// <summary>
        /// Invoked to post configure a KdSoftAuthenticationOptions instance.
        /// </summary>
        /// <param name="name">The name of the options instance being configured.</param>
        /// <param name="options">The options instance to configure.</param>
        public void PostConfigure(string name, KdSoftAuthenticationOptions options) {
            if (options.TokenHandler == null) {
                options.TokenHandler = new JwtSecurityTokenHandler();
            }
            if (options.ValidationParameters == null) {
                options.ValidationParameters = new TokenValidationParameters {
                    ValidIssuer = string.IsNullOrWhiteSpace(options.JwtIssuer) ? "" : options.JwtIssuer,
                    ValidateIssuer = !string.IsNullOrWhiteSpace(options.JwtIssuer),
                    ValidAudiences = string.IsNullOrWhiteSpace(options.JwtAudience) ? new string[0] : new string[] { options.JwtAudience },
                    RequireSignedTokens = true,
                    RequireExpirationTime = true,
                    IssuerSigningKey = options.JwtCredentials.Key,
                };
            }
        }
    }
}
