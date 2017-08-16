using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace KdSoft.Services.WebApi.Infrastructure
{
    /* Example for how to configure Asp.Net Core, by replacing existing SimpleTypeModelBinderProvider
        var mvc = services.AddMvc(options => {
            var indx = options.ModelBinderProviders.FindIndex(mbp => mbp is SimpleTypeModelBinderProvider);
            if (indx >= 0)
                options.ModelBinderProviders[indx] = new SimpleTypeIsoModelBinderProvider();
            else
                options.ModelBinderProviders.Insert(0, new SimpleTypeIsoModelBinderProvider());
        });
     */

    /// <summary>
    /// Replacement for <see cref="SimpleTypeModelBinderProvider"/> that overrides <see cref="TimeSpan"/>
    /// binding by using the <see cref="IsoTimeSpanBinder"/>.
    /// </summary>
    public class SimpleTypeIsoModelBinderProvider: IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            if (!context.Metadata.IsComplexType) {
                var mt = context.Metadata.ModelType;
                if (mt == typeof(TimeSpan) || mt == typeof(TimeSpan?))
                    return new IsoTimeSpanBinder();
                else
                    return new SimpleTypeModelBinder(context.Metadata.ModelType);
            }

            return null;
        }
    }
}
