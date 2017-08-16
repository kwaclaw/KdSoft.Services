using System;
using System.Net;
using KdSoft.Data.Models.Shared;
using KdSoft.Utils;
using Microsoft.AspNetCore.Mvc;

namespace KdSoft.Services.WebApi
{
    public static class Utils
    {
        // usefule when exception cannot be serialized by the chosen Serialization framework
        public static ObjectResult CreateServiceErrorResult(HttpStatusCode statusCode, Exception exception) {
            var message = exception.Message;
            if (exception is AggregateException) {
                message = ((AggregateException)exception).CombineMessages();
            }
            var error = new ServiceError { Code = (int)statusCode, Message = message };
            return new ObjectResult(error) { StatusCode = (int)statusCode };
        }

        // usefule when exception cannot be serialized by the chosen Serialization framework
        public static ObjectResult CreateServiceErrorResult(HttpStatusCode statusCode, string message) {
            var error = new ServiceError { Code = (int)statusCode, Message = message };
            return new ObjectResult(error) { StatusCode = (int)statusCode };
        }
    }
}
