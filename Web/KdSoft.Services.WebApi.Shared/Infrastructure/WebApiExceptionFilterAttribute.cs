using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;

namespace KdSoft.Services.WebApi.Infrastructure
{
    // We use this because some exceptions cannot be serialized by some serialization frameworks.
    public class WebApiExceptionFilterAttribute: ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context) {
            var exception = context.Exception;
            // set the response to our friendly ServiceError
            context.Result = Utils.CreateServiceErrorResult(HttpStatusCode.InternalServerError, exception);
            base.OnException(context);
        }
    }
}
