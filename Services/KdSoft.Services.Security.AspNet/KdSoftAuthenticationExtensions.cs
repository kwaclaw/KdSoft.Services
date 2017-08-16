using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace KdSoft.Services.Security.AspNet
{
    public static class KdSoftAuthenticationExtensions
    {
        public static AuthenticationBuilder AddKdSoft(this AuthenticationBuilder builder)
            => builder.AddKdSoft(AuthenticationSchemes.KdSoft);


        public static AuthenticationBuilder AddKdSoft(this AuthenticationBuilder builder, string authenticationScheme)
            => builder.AddKdSoft(authenticationScheme, configureOptions: null);


        public static AuthenticationBuilder AddKdSoft(this AuthenticationBuilder builder, Action<KdSoftAuthenticationOptions> configureOptions)
            => builder.AddKdSoft(AuthenticationSchemes.KdSoft, configureOptions);


        public static AuthenticationBuilder AddKdSoft(this AuthenticationBuilder builder, string authenticationScheme, Action<KdSoftAuthenticationOptions> configureOptions)
            => builder.AddKdSoft(authenticationScheme, displayName: null, configureOptions: configureOptions);


        public static AuthenticationBuilder AddKdSoft(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<KdSoftAuthenticationOptions> configureOptions) {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<KdSoftAuthenticationOptions>, KdSoftAuthenticationPostConfigureOptions>());
            return builder.AddScheme<KdSoftAuthenticationOptions, KdSoftAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
        }
    }
}
