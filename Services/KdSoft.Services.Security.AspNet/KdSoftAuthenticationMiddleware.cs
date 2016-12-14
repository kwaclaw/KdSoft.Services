using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Encodings.Web;

namespace KdSoft.Services.Security.AspNet
{
    public class KdSoftAuthenticationMiddleware: AuthenticationMiddleware<KdSoftAuthenticationOptions>
    {
        readonly TokenValidationParameters validationParameters;
        readonly JwtSecurityTokenHandler tokenHandler;
        readonly ILogger logger;
        readonly IStringLocalizer<KdSoftAuthenticationMiddleware> localizer;

        public KdSoftAuthenticationMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, UrlEncoder encoder, IOptions<KdSoftAuthenticationOptions> options, IStringLocalizer<KdSoftAuthenticationMiddleware> localizer)
            : base(next, options, loggerFactory, encoder)
        {
            this.validationParameters = new TokenValidationParameters
            {
                ValidIssuer = string.IsNullOrWhiteSpace(Options.JwtIssuer) ? "" : Options.JwtIssuer,
                ValidateIssuer = !string.IsNullOrWhiteSpace(Options.JwtIssuer),
                ValidAudiences = string.IsNullOrWhiteSpace(Options.JwtAudience) ? new string[0] : new string[] { Options.JwtAudience },
                RequireSignedTokens = true,
                RequireExpirationTime = true,
                IssuerSigningKey = Options.JwtCredentials.Key,
            };

            this.tokenHandler = new JwtSecurityTokenHandler();
            this.logger = loggerFactory.CreateLogger<KdSoftAuthenticationHandler>();
            this.localizer = localizer;
        }

        protected override AuthenticationHandler<KdSoftAuthenticationOptions> CreateHandler() {
            return new KdSoftAuthenticationHandler(validationParameters, tokenHandler, logger, localizer);
        }
    }

    public static class KdSoftAuthenticationExtensions
    {
        public static IApplicationBuilder UseKdSoftAuthentication(this IApplicationBuilder app, KdSoftAuthenticationOptions options) {
            app.UseMiddleware<KdSoftAuthenticationMiddleware>(Options.Create(options));
            return app;
        }
    }
}
