using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace KdSoft.Services.WebApi
{
    public class FileCopyResultExecutor : FileResultExecutorBase
    {
        public FileCopyResultExecutor(ILoggerFactory loggerFactory)
            : base(CreateLogger<FileCopyResultExecutor>(loggerFactory)) {
        }

        public Task ExecuteAsync(ActionContext context, FileCopyResult result) {
            SetHeadersAndLog(context, result);
            return WriteFileAsync(context, result);
        }

        private static Task WriteFileAsync(ActionContext context, FileCopyResult result) {
            var bufferingFeature = context.HttpContext.Features.Get<IHttpBufferingFeature>();
            bufferingFeature?.DisableResponseBuffering();

            var response = context.HttpContext.Response;
            return result.CopyAsync(response.Body);
        }
    }
}
