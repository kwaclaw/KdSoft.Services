using KdSoft.Utils;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace KdSoft.Services.WebApi.Infrastructure
{
    /// <summary>
    /// <see cref="IModelBinder"/> for deserializing ISO durations as <see cref="TimeSpan"/> instances.
    /// </summary>
    public class IsoTimeSpanBinder: IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext) {
            if (bindingContext == null) {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None) {  // no entry
                return TaskCache.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            try {
                var value = valueProviderResult.FirstValue;
                TimeSpan? result = null;
                if (!string.IsNullOrWhiteSpace(value)) {
                    TimeSpan parsed;
                    if (TimeSpanExtensions.TryParseIso(value, out parsed)) {
                        result = parsed;
                        bindingContext.Result = ModelBindingResult.Success(result);
                    }
                    else {
                        bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "ISO 8601 duration format error.");
                    }
                }
                else {
                    bindingContext.Result = ModelBindingResult.Success(result);
                }
            }
            catch (Exception ex) {
                var isFormatException = ex is FormatException;
                if (!isFormatException && ex.InnerException != null) {
                    // TypeConverter throws System.Exception wrapping the FormatException, so we capture the inner exception.
                    ex = ExceptionDispatchInfo.Capture(ex.InnerException).SourceException;
                }
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, ex, bindingContext.ModelMetadata);
            }
            return TaskCache.CompletedTask;
        }
    }
}
