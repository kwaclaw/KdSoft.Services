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
    public class QLineAuthenticationMiddleware: AuthenticationMiddleware<QLineAuthenticationOptions>
    {
        readonly TokenValidationParameters validationParameters;
        readonly JwtSecurityTokenHandler tokenHandler;
        readonly ILogger logger;
        readonly IStringLocalizer<QLineAuthenticationMiddleware> localizer;

        public QLineAuthenticationMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, UrlEncoder encoder, IOptions<QLineAuthenticationOptions> options, IStringLocalizer<QLineAuthenticationMiddleware> localizer)
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
            this.logger = loggerFactory.CreateLogger<QLineAuthenticationHandler>();
            this.localizer = localizer;
        }

        protected override AuthenticationHandler<QLineAuthenticationOptions> CreateHandler() {
            return new QLineAuthenticationHandler(validationParameters, tokenHandler, logger, localizer);
        }
    }

    public static class QLineAuthenticationExtensions
    {
        public static IApplicationBuilder UseQLineAuthentication(this IApplicationBuilder app, QLineAuthenticationOptions options) {
            app.UseMiddleware<QLineAuthenticationMiddleware>(Options.Create(options));
            return app;
        }
    }
}
