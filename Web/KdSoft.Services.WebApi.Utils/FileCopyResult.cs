using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace KdSoft.Services.WebApi
{
    public class FileCopyResult: FileResult
    {
        Func<Stream, Task> copyAsync;

        public FileCopyResult(Func<Stream, Task> copyAsync, string contentType) : base(contentType) {
            if (copyAsync == null)
                throw new ArgumentNullException(nameof(copyAsync));
            this.copyAsync = copyAsync;
        }

        public Func<Stream, Task> CopyAsync {
            get { return copyAsync; }
            set {
                if (value == null) {
                    throw new ArgumentNullException(nameof(value));
                }
                copyAsync = value;
            }
        }

        // see  github.com/aspnet/mvc: src/Microsoft.AspNetCore.Mvc.Core/Internal/FileStreamResultExecutor.cs
        public override Task ExecuteResultAsync(ActionContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            var executor = context.HttpContext.RequestServices.GetRequiredService<FileCopyResultExecutor>();
            return executor.ExecuteAsync(context, this);
        }
    }
}
